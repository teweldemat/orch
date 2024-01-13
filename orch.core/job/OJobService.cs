using Hangfire;
using Hangfire.Server;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using orch.core.errors;
using orch.core.job;
using orch.core.model;
using System.Collections.Concurrent;

namespace orch.core
{
    public sealed partial class OJobService
    {
        private readonly IApplicationScopeFactory _scopeFactory;

        private readonly OTransactionService _transactionService;
        private readonly IHubContext<JobProgressHub> _hubContext;
        private readonly IOHost _host;

        public OJobService(
            IApplicationScopeFactory scopeFactory,
            OTransactionService transactionService,
            IHubContext<JobProgressHub> hubContext,
            IOHost host)
        {
            _scopeFactory = scopeFactory;
            _transactionService = transactionService;
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

            handler = GetHandler(job.DataTypeID);
            handler.SetData(job, data);
            handler.HubContext = _hubContext;
            handler.Cts = cts;

            job.Id = context.BackgroundJob.Id;
            job.TextData = JsonConvert.SerializeObject(data);
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

                        var jobId = BackgroundJob.Schedule(() => EnqueueJob(default, job, data), TimeSpan.FromSeconds(3));
                        singletonJobsByTypeId.TryAdd(typeId, jobId);

                        return jobId;
                    }
                case JobProcessType.Concurrent:
                    {
                        var jobId = BackgroundJob.Enqueue(() => EnqueueJob(default, job, data));
                        concurrentJobsByJobId.TryAdd(jobId, typeId);
                        return jobId;
                    }
                default:
                    throw new InvalidOperationException("Invalid ProcessType");
            }

        }


        [AutomaticRetry(Attempts = 0)]
        public async Task EnqueueJob(PerformContext context, OJob job, object data)
        {
            try
            {
                ProcessJob(context, job, data, out IJobHandler handler);

                // PerformContext is a special argument type which Hangfire will substitute automatically
                await handler.Execute();

                job.TextSummary = handler.Summarize();

                _transactionService.Db.AddJob(job);

            }
            finally
            {
                if (singletonJobsByTypeId.ContainsKey(job.DataTypeID))
                {
                    singletonJobsByTypeId.TryRemove(job.DataTypeID, out _);
                }
                else if (concurrentJobsByJobId.ContainsKey(job.Id))
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

            if (concurrentJobsByJobId.ContainsKey(jobId) || singletonJobsByTypeId.Values.Contains(jobId))
            {
                if (BackgroundJob.Delete(jobId))
                {
                    if (cancellationTokenSources.TryGetValue(jobId, out var cts))
                    {
                        cts.Cancel();
                    }

                    concurrentJobsByJobId.TryRemove(jobId, out _);
                    singletonJobsByTypeId.TryRemove(singletonJobsByTypeId.FirstOrDefault(x => x.Value == jobId).Key, out _);
                    return true;
                }
            }

            return false;
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
            if (typeInfo?.Permissions?.Length != null
                && typeInfo.Permissions.Length > 0 && !_transactionService.IsRootUser(userId))
            {
                if (!_transactionService.Db.IsPermitted(userId, typeInfo.Permissions, out var notGrantedPermissions))
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
}
