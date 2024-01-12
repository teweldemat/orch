using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using orch.core.model;

namespace orch.core.job
{
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

        public CancellationTokenSource Cts { get; set; }

        public bool IsCancelled => Cts?.IsCancellationRequested ?? false;

        async Task IJobHandler.Execute()
        {

            await Execute();

            if (IsCancelled)
            {
                OnCancelled();
                HubContext.Clients.Group(_jobInfo.Id)?.SendAsync(JobProgressHub.CancelledMethod, _jobInfo.Id);
            }
            else
            {
                await HubContext.Clients.Group(_jobInfo.Id).SendAsync(JobProgressHub.SuccessMethod);
            }

        }
        string IJobHandler.Summarize()
        {
            if (_jobData is null)
                return string.Empty;

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

        public abstract Task Execute();

        public virtual void OnCancelled() { }

        public abstract string Summarize(out bool html);
    }
}