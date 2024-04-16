using Hangfire;
using Hangfire.Server;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using orch.core.errors;
using orch.core.job;
using orch.core.logging;
using orch.core.model;
using System.Collections.Concurrent;

namespace orch.core
{
    public sealed partial class OJobService
    {
        private IApplicationScopeFactory ScopeFactory { get; set; }
        private IOHost Host { get; }
        private OTransactionService TranService { get; }
        private ITransactionDatabase TranDb { get; }
        private IEventLogDatabase EventLogDb { get; }
        private IHubContext<JobProgressHub> HubContext { get; }
        private IRecurringJobManager RecurringJobManager { get; }

        public OJobService(
            IApplicationScopeFactory scopeFactory,
            IOHost host,
            OTransactionService tranService,
            ITransactionDatabase tranDb,
            IEventLogDatabase eventLogDb,
            IHubContext<JobProgressHub> hubContext,
            IRecurringJobManager recurringJobManager)
        {
            ScopeFactory = scopeFactory;
            Host = host;
            TranService = tranService;
            TranDb = tranDb;
            EventLogDb = eventLogDb;
            HubContext = hubContext;
            RecurringJobManager = recurringJobManager;
        }

        private static readonly ConcurrentDictionary<Guid, string> singletonJobsByTypeId = new();
        private static readonly ConcurrentDictionary<string, Guid> concurrentJobsByJobId = new();

        private void ProcessJob(PerformContext context, OJob job, object data, out IJobHandler handler, CancellationToken cancellationToken)
        {
            job.Id = context.BackgroundJob.Id;
            job.TextData = JsonConvert.SerializeObject(data);

            handler = GetHandler(job.DataTypeID);
            handler.SetData(job, data);
            handler.HubContext = HubContext;
            handler.CancellationToken = cancellationToken;
        }

        public string EnqueueJob(Guid userId, Guid systemId, Guid typeId, object data)
        {
            var typeInfo = GetTypeInfoById(typeId) ?? throw new JobTypeIdNotFoundException(typeId);

            Authorize(typeInfo, userId);

            var job = new OJob()
            {
                UserId = userId,
                SystemID = systemId,
                Time = Host.CurrentTime(),
                DataTypeID = typeId,
            };

            switch (typeInfo.ProcessType)
            {
                case JobProcessType.Singleton:
                    {
                        if (singletonJobsByTypeId.TryGetValue(typeId, out var existingJobId))
                            return existingJobId;

                        var jobId = BackgroundJob.Enqueue(() => ExecuteJob(default, job, data, CancellationToken.None));
                        singletonJobsByTypeId.TryAdd(typeId, jobId);

                        return jobId;
                    }
                case JobProcessType.Concurrent:
                    {
                        var jobId = BackgroundJob.Enqueue(() => ExecuteJob(default, job, data, CancellationToken.None));
                        concurrentJobsByJobId.TryAdd(jobId, typeId);
                        return jobId;
                    }
                default:
                    throw new InvalidOperationException($"Job Type '{typeInfo.ProcessType}' cannot be enqueued.");
            }

        }

        /// <param name="context">PerformContext is a special argument type which Hangfire will substitute automatically</param>
        /// <param name="cancellationToken">CancellationToken is a special argument type which Hangfire will substitute automatically</param>
        [AutomaticRetry(Attempts = 0)]
        public async Task ExecuteJob(PerformContext context, OJob job, object data, CancellationToken cancellationToken)
        {
            var typeInfo = GetTypeInfoById(job.DataTypeID);

            try
            {
                ProcessJob(context, job, data, out IJobHandler handler, cancellationToken);

                await handler.Execute();

                job.TextSummary = handler.Summarize();

                TranService.Db.AddJob(job);

            }
            catch (Exception ex)
            {
                EventLogDb.Add(new EventLog
                {
                    Id = Host.NextGuid(),
                    Time = Host.CurrentTime(),
                    Message = $"An error occured while running Job {context.BackgroundJob.Id} - '{typeInfo.Key}': {ex.Message}",
                    Level = EventLogProps.LogLevel.Error,
                    JobId = context.BackgroundJob.Id,
                    Data = null
                });

                throw;
            }
            finally
            {
                if (typeInfo.ProcessType == JobProcessType.Singleton)
                {
                    singletonJobsByTypeId.TryRemove(job.DataTypeID, out _);
                }
                else if (typeInfo.ProcessType == JobProcessType.Concurrent)
                {
                    concurrentJobsByJobId.TryRemove(context.BackgroundJob.Id, out _);
                }
            }
        }

        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(60)]
        public Task ExecuteRecurringJob(
            PerformContext context, OJob job, object data, CancellationToken cancellationToken)
        {
            return ExecuteJob(context, job, data, cancellationToken);
        }

        public bool CancelJob(Guid userId, string jobId)
        {
            Guid typeId;

            if (singletonJobsByTypeId.Values.Contains(jobId))
            {
                typeId = singletonJobsByTypeId.FirstOrDefault(x => x.Value == jobId).Key;
            }
            else
            {
                concurrentJobsByJobId.TryGetValue(jobId, out typeId);
            }

            if (typeId == Guid.Empty)
            {
                throw new InvalidOperationException($"Cannot cancel job '{jobId}' as it does not exist or has already been cancelled.");
            }

            var typeInfo = GetTypeInfoById(typeId) ?? throw new JobTypeIdNotFoundException(typeId);

            Authorize(typeInfo, userId);

            if (!BackgroundJob.Delete(jobId))
            {
                throw new ApplicationException($"An error occured while cancelling job '{jobId}'.");
            }

            return true;
        }

        public static string? GetActiveSingletonJobId(Guid typeId)
        {
            _ = GetTypeInfoById(typeId) ?? throw new JobTypeIdNotFoundException(typeId);

            if (singletonJobsByTypeId.TryGetValue(typeId, out var jobId))
            {
                return jobId;
            }

            return null;
        }

        private void Authorize(JobTypeInfo typeInfo, Guid userId)
        {
            if (typeInfo?.Permissions?.Length is not null
                && typeInfo.Permissions.Any()
                    && !TranService.IsRootUser(userId))
            {
                if (!TranService.Db.IsPermitted(userId, typeInfo.Permissions, out var notGrantedPermissions))
                {
                    var notGrantedPermissionsStr = string.Join(", ", notGrantedPermissions);
                    throw new UnauthorizedAccessException($"You are not authorized to initiate or cancel job: '{typeInfo.TypeName}'. Missing permissions: {notGrantedPermissionsStr}");
                }
            }
        }
        
        

        public void AddOrUpdateRecurringJobs()
        {
            var systemUser = TranDb.GetSystemUser()
             ?? throw new InvalidOperationException("Unable to retrieve system user information.");

            var sysInfo = TranDb.GetCurrentSystemInformation()
                ?? throw new InvalidOperationException("Unable to retrieve current system information.");

            foreach (var typeInfo in GetAllJobTypes().Where(jt => jt.ProcessType is JobProcessType.Recurring))
            {
                if (GetPredicate(typeInfo.TypeId) is { } predicate)
                {
                    if (!predicate.CanRun())
                    {
                        RecurringJob.RemoveIfExists(typeInfo.Key);
                        continue;
                    }

                }
                
                var job = new OJob()
                {
                    UserId = systemUser.Id,
                    SystemID = sysInfo.SystemId,
                    Time = Host.CurrentTime(),
                    DataTypeID = typeInfo.TypeId,
                };

                RecurringJob.AddOrUpdate<OJobService>(
                    typeInfo.Key,
                    (service) => service.ExecuteRecurringJob(default, job, null, CancellationToken.None),
                    typeInfo.Cron);
            }
        }
    }
}

