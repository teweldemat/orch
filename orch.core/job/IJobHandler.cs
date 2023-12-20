using Microsoft.AspNetCore.SignalR;
using orch.core.model;

namespace orch.core.job
{
    public interface IJobHandler
    {
        internal IHubContext<JobProgressHub> HubContext { get; set; }
        internal CancellationTokenSource Cts { get; set; }
        public void SetData(OJob job, object data);
        public void Execute();
        public string Summarize();
    }
}
