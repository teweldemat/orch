using Hangfire;
using Hangfire.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IApplicationScopeFactory _scopeFactory;
        private readonly OTransactionService _tranService;
        private readonly ITransactionDatabase _tranDb;
        private readonly IEventLogDatabase _eventLogDb;
        private readonly IHubContext<JobProgressHub> _hubContext;
        private readonly IOHost _host;

        public OJobService(
            IApplicationScopeFactory scopeFactory,
            OTransactionService tranService,
            ITransactionDatabase tranDb,
            IEventLogDatabase eventLogDb,
            IHubContext<JobProgressHub> hubContext,
            IOHost host)
        {
            _scopeFactory = scopeFactory;
            _tranService = tranService;
            _tranDb = tranDb;
            _eventLogDb = eventLogDb;
            _hubContext = hubContext;
            _host = host;
        }

        private IServiceProvider? _services;
        public IServiceProvider Services
        {
            get
            {
                return _services ??= _scopeFactory.CreateApplicationScope().Services;
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
            handler.HubContext = _hubContext;
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
                Time = _host.CurrentTime(),
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
                    throw new InvalidOperationException($"Job Type '{nameof(JobProcessType)}' not supported.");
            }

        }


        [AutomaticRetry(Attempts = 0)]
        public async Task ExecuteJob(PerformContext context, OJob job, object data)
        {
            try
            {
                ProcessJob(context, job, data, out IJobHandler handler);

                // PerformContext is a special argument type which Hangfire will substitute automatically
                await handler.Execute();

                job.TextSummary = handler.Summarize();

                _tranService.Db.AddJob(job);

            }
            catch (Exception ex)
            {
                var typeInfo = GetTypeInfoById(job.DataTypeID);

                _eventLogDb.Add(new EventLog
                {
                    Id = _host.NextGuid(),
                    Time = _host.CurrentTime(),
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
                    && !_tranService.IsRootUser(userId))
            {
                if (!_tranService.Db.IsPermitted(userId, typeInfo.Permissions, out var notGrantedPermissions))
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
    }

    public static class OJobServiceHelpers
    {
        public static void AddRecurringJobs(this IApplicationBuilder app)
        {
            var serviceProvider = app.ApplicationServices;
            _ = serviceProvider.GetRequiredService<IRecurringJobManager>();
            var tranDb = serviceProvider.GetRequiredService<ITransactionDatabase>();
            var host = serviceProvider.GetRequiredService<IOHost>();

            foreach (var typeInfo in OJobService.GetAllJobTypes().Where(jt => jt.ProcessType is JobProcessType.Recurring))
            {
                var job = new OJob()
                {
                    UserId = tranDb.GetSystemUser().Id,
                    SystemID = tranDb.GetCurrentSystemInformation().SystemId,
                    Time = host.CurrentTime(),
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

