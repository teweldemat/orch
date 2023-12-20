using Microsoft.Extensions.DependencyInjection;
using orch.core;

namespace orch.wf
{
    public class WfServiceCollection : TransactionServiceCollection
    {
        public WfService WfService =>
            _tranService.Services.GetService<WfService>() ?? throw new NullReferenceException($"{nameof(wf.WfService)} service couldn't be retrieved");
        public IWfDatabase WfDb =>
            _tranService.Services.GetService<IWfDatabase>() ?? throw new NullReferenceException($"{nameof(IWfDatabase)} service couldn't be retrieved");
        public WfNotificationService WfNotificationService =>
            _tranService.Services.GetService<WfNotificationService>() ?? throw new NullReferenceException($"{nameof(wf.WfNotificationService)} service couldn't be retrieved");

        public WfServiceCollection(IOHost host,
            OTransactionService tranService) : base(host, tranService)
        {
        }
    }
}
