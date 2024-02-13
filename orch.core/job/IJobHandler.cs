using Microsoft.AspNetCore.SignalR;
using orch.core.model;

namespace orch.core.job
{
    public interface IJobHandler
    {
        internal IHubContext<JobProgressHub> HubContext { get; set; }
        internal CancellationToken CancellationToken { get; set; }
        public void SetData(OJob job, object data);
        public Task Execute();
        public string Summarize();
    }
}
