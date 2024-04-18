using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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

        async Task IJobHandler.Execute()
        {
            try
            {
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

            if (html)
            {
                return ret;
            }

            return $"<p>{System.Web.HttpUtility.HtmlEncode(ret)}</p>";
        }
        public void SetData(OJob job, object data)
        {
            _jobInfo = job;

            if (data is Newtonsoft.Json.Linq.JObject jObjectData)
                _jobData = jObjectData.ToObject<T>();
            else
                _jobData = (T)data;
        }

        private readonly JsonSerializerSettings settings = new()
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
                JsonConvert.SerializeObject(progress, settings)).Wait();
        }

        protected void AddEventLog(
            EventLogProps.LogLevel level,
            string message,
            string reference = null,
            object data = null)
        {
            var scope = _services.TranService.Services.CreateScope();
            var tranDb = scope.ServiceProvider.GetRequiredService<ITransactionDatabase>();
            var eventLogDb = scope.ServiceProvider.GetRequiredService<IEventLogDatabase>();

            try
            {
                tranDb.BeginTransaction();
                
                var eventLog = new EventLog
                {
                    Id = _services.Host.NextGuid(),
                    Time = _services.Host.CurrentTime(),
                    Message = message,
                    Level = level,
                    Reference = reference,
                    JobId = _jobInfo.Id,
                    Data = data is null ? null : JsonConvert.SerializeObject(data)
                };

                eventLogDb.Add(eventLog);

                tranDb.CommitTransaction();
            }
            catch (Exception ex)
            {
                tranDb.RollbackTransaction();
            }
            finally
            {
                scope.Dispose();
            }
        }

        public abstract Task Execute();

        public virtual void OnCancelled() { }

        public abstract string Summarize(out bool html);
    }
}