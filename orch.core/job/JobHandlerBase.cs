using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using orch.core.command;
using orch.core.logging;
using orch.core.model;

namespace orch.core.job
{
    public abstract class JobHandlerBase<T> : JobHandlerBase<T, JobProgress> where T : class
    {
        protected JobHandlerBase(TransactionServiceCollection services) : base(services)
        {
        }
    }

    public abstract class JobHandlerBase<T, P> : IJobHandler where P : JobProgress
    {
        protected TransactionServiceCollection _services;

        protected OJob _jobInfo;
        protected T _jobData;


        protected JobHandlerBase(TransactionServiceCollection services)
        {
            _services = services;
        }

        public IHubContext<JobProgressHub> HubContext { get; set; }

        public CancellationToken CancellationToken { get; set; }

        protected bool IsCancelled => CancellationToken.IsCancellationRequested;
        
        
        private UserInfo? _systemUser;
        private UserInfo SystemUser => _systemUser 
            ??= _services.TranDb.GetSystemUser() 
                ?? throw new InvalidOperationException("System user not found. Has the system been bootstrapped?");

        async Task IJobHandler.Execute()
        {
            try
            {
                _ = SystemUser;
                
                await Execute();

                await HubContext.Clients.Group(_jobInfo.Id)
                    .SendAsync(JobProgressHub.SuccessMethod, cancellationToken: CancellationToken);
            }
            finally
            {

                if (IsCancelled)
                {
                    HubContext.Clients.Group(_jobInfo.Id)?.SendAsync(JobProgressHub.CancelledMethod, _jobInfo.Id,
                        cancellationToken: CancellationToken);
                    OnCancelled();
                }
            }

        }
        string IJobHandler.Summarize()
        {
            var ret = Summarize(out var html);
            return html ? ret : $"<p>{System.Web.HttpUtility.HtmlEncode(ret)}</p>";
        }
        public void SetData(OJob job, object data)
        {
            _jobInfo = job;

            if (data is Newtonsoft.Json.Linq.JObject jObjectData)
                _jobData = jObjectData.ToObject<T>();
            else
                _jobData = (T)data;
        }

        private readonly JsonSerializerSettings _settings = new()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = null
            }
        };

        protected void SendProgress(P progress)
        {
            HubContext.Clients.Group(_jobInfo.Id).SendAsync(
                JobProgressHub.ProgressMethod,
                JsonConvert.SerializeObject(progress, _settings)).Wait();
        }

        public abstract Task Execute();

        public virtual void OnCancelled() { }

        public abstract string Summarize(out bool html);

        #region helpers
        
        protected void ExecuteCommand<DataType>(int formatVersion, DataType data, out Guid tranId)
        {
            using var scope = _services.TranService.Services.CreateScope();
            var transactionService = scope.ServiceProvider.GetRequiredService<OTransactionService>();
            transactionService.ExecuteCommand<DataType>(
                _jobInfo.UserId,
                _jobInfo.SystemID,
                formatVersion,
                data,
                out tranId);
        }

        protected void ExecuteCommandUntyped(Guid typeId, int formatVersion, object data, out Guid tranId)
        {
            using var scope = _services.TranService.Services.CreateScope();
            var transactionService = scope.ServiceProvider.GetRequiredService<OTransactionService>();
            transactionService.ExecuteCommandUntyped(
                _jobInfo.UserId,
                _jobInfo.SystemID,
                typeId,
                formatVersion,
                data,
                out tranId);
        }

        protected void AddEventLog(
            EventLogProps.LogLevel level,
            string message,
            string reference = null,
            object data = null)
        {
            
            using var scope = _services.TranService.Services.CreateScope();
            var transactionService = scope.ServiceProvider.GetRequiredService<OTransactionService>();
            transactionService.ExecuteCommandUntyped(
                SystemUser.Id,
                _jobInfo.SystemID,
                Guid.Parse(AddEventLogCommand.TYPE_ID),
                0,
                new AddEventLogCommand()
                {
                    EventLog = new EventLog()
                    {
                        JobId = _jobInfo.Id,
                        Level = level,
                        Message = message,
                        Reference = reference,
                        Data = JsonConvert.SerializeObject(data)
                    }
                },
                out _);
        }

        #endregion
    }
}