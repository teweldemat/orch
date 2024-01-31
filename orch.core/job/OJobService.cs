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
        private IApplicationScopeFactory ScopeFactory { get; }
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
            TranService = tranService;
            TranDb = tranDb;
            EventLogDb = eventLogDb;
            HubContext = hubContext;
            Host = host;
            this.RecurringJobManager = recurringJobManager;
        }

        private IServiceProvider? _services;
        public IServiceProvider Services
        {
            get
            {
                return _services ??= ScopeFactory.CreateApplicationScope().Services;
            }
        }

        private static readonly ConcurrentDictionary<Guid, string> singletonJobsByTypeId = new();
        private static readonly ConcurrentDictionary<string, Guid> concurrentJobsByJobId = new();

        private static readonly ConcurrentDictionary<string, CancellationTokenSource> cancellationTokenSources = new();

        private void ProcessJob(PerformContext context, OJob job, object data, out IJobHandler handler)
        {
            CancellationTokenSource cts = new();
            cancellationTokenSources.TryAdd(context.BackgroundJob.Id, cts);

            job.Id = context.BackgroundJob.Id;
            job.TextData = JsonConvert.SerializeObject(data);

            handler = GetHandler(job.DataTypeID);
            handler.SetData(job, data);
            handler.HubContext = HubContext;
            handler.Cts = cts;
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

                        // TODO: hack - it shoudln't need to be scheduled
                        var jobId = BackgroundJob.Enqueue(() => ExecuteJob(default, job, data));
                        singletonJobsByTypeId.TryAdd(typeId, jobId);

                        return jobId;
                    }
                case JobProcessType.Concurrent:
                    {
                        var jobId = BackgroundJob.Enqueue(() => ExecuteJob(default, job, data));
                        concurrentJobsByJobId.TryAdd(jobId, typeId);
                        return jobId;
                    }
                default:
                    throw new InvalidOperationException($"Job Type '{typeInfo.ProcessType}' cannot be enqueued.");
            }

        }

        /// <param name="context">PerformContext is a special argument type which Hangfire will substitute automatically</param>
        [AutomaticRetry(Attempts = 0)]
        public async Task ExecuteJob(PerformContext context, OJob job, object data)
        {
            try
            {
                ProcessJob(context, job, data, out IJobHandler handler);

                await handler.Execute();

                job.TextSummary = handler.Summarize();

                TranService.Db.AddJob(job);

            }
            catch (Exception ex)
            {
                var typeInfo = GetTypeInfoById(job.DataTypeID);

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
                if (singletonJobsByTypeId.ContainsKey(job.DataTypeID))
                {
                    singletonJobsByTypeId.TryRemove(job.DataTypeID, out _);
                }
                else if (job.Id is not null && concurrentJobsByJobId.ContainsKey(job.Id))
                {
                    concurrentJobsByJobId.TryRemove(job.Id, out _);
                }
            }
        }

        public bool CancelJob(Guid userId, string jobId)
        {
            Guid typeId = GetTypeIdForJobId(jobId);

            var typeInfo = GetTypeInfoById(typeId) ?? throw new JobTypeIdNotFoundException(typeId);

            Authorize(typeInfo, userId);

            if (!concurrentJobsByJobId.ContainsKey(jobId) && !singletonJobsByTypeId.Values.Contains(jobId))
            {
                throw new InvalidOperationException($"Cannot cancel job '{jobId}' as it does not exist or has already been cancelled.");
            }

            var cancelled = BackgroundJob.Delete(jobId);

            if (!cancelled)
            {
                throw new ApplicationException($"Failed to cancel job '{jobId}'.");
            }

            if (cancellationTokenSources.TryGetValue(jobId, out var cts))
            {
                cts.Cancel();
            }

            concurrentJobsByJobId.TryRemove(jobId, out _);
            singletonJobsByTypeId.TryRemove(singletonJobsByTypeId.FirstOrDefault(x => x.Value == jobId).Key, out _);
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

        private static Guid GetTypeIdForJobId(string jobId)
        {
            if (singletonJobsByTypeId.Values.Contains(jobId))
            {
                return singletonJobsByTypeId.FirstOrDefault(x => x.Value == jobId).Key;
            }

            if (concurrentJobsByJobId.TryGetValue(jobId, out var typeId))
            {
                return typeId;
            }

            throw new InvalidOperationException("Job ID does not exist or is not active.");
        }

        public void AddOrUpdateRecurringJobs()
        {
            var systemUser = TranDb.GetSystemUser()
             ?? throw new InvalidOperationException("Unable to retrieve system user information.");

            var sysInfo = TranDb.GetCurrentSystemInformation()
                ?? throw new InvalidOperationException("Unable to retrieve current system information.");

            foreach (var typeInfo in GetAllJobTypes().Where(jt => jt.ProcessType is JobProcessType.Recurring))
            {
                var job = new OJob()
                {
                    UserId = systemUser.Id,
                    SystemID = sysInfo.SystemId,
                    Time = Host.CurrentTime(),
                    DataTypeID = typeInfo.TypeId,
                };

                RecurringJob.AddOrUpdate<OJobService>(
                    typeInfo.Key,
                    (service) => service.ExecuteJob(default, job, null),
                    typeInfo.Cron);
            }
        }
    }
}

