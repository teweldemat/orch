using FuncScript;
using FuncScript.Core;
using orch.core;
using orch.core.model;
using orch.wf.model;

namespace orch.wf
{
    public class WfProvider : FuncScript.Core.IFsDataProvider
    {
        DefaultFsDataProvider global = new DefaultFsDataProvider();
        common.CachedObject<string, object> _cache;
        WfServiceCollection _services;
        OTask _task;
        WfStateData _wfState;
        UserInfo _checkUser;
        ViewQueryProvider _viewProvider;
        public WfProvider(
            WfServiceCollection services,
            UserInfo user,
            OTask task,
            WfStateData wfState,
            object action)
        {
            _services = services;
            _task = task;
            _wfState = wfState;
            _checkUser = user;
            _viewProvider = new ViewQueryProvider(_services.TranService, user == null ? Guid.Empty : user.Id, null);
            _cache =
                new common.CachedObject<string, object>(name =>
                {
                    switch (name)
                    {
                        case "user":
                            return Engine.NormalizeDataType(_checkUser);
                        case "task":
                            return Engine.NormalizeDataType(_task);
                        case "wfstate":
                            return Engine.NormalizeDataType(_wfState);
                        case "action":
                            return Engine.NormalizeDataType(action);
                        default:
                            break;
                    }
                    return _viewProvider.Get(name);
                });
        }
        public object Get(string name)
        {
            return _cache[name];
        }

        public IFsDataProvider ParentProvider => null;
        public bool IsDefined(string key) => true;
    }
}
