using funcscript;
using orch.core;
using orch.core.model;
using orch.wf.model;

namespace orch.wf
{
    public class WfProvider : funcscript.core.IFsDataProvider
    {
        DefaultFsDataProvider global = new funcscript.DefaultFsDataProvider();
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
                            return FuncScript.NormalizeDataType(_checkUser);
                        case "task":
                            return FuncScript.NormalizeDataType(_task);
                        case "wfstate":
                            return FuncScript.NormalizeDataType(_wfState);
                        case "action":
                            return FuncScript.NormalizeDataType(action);
                        default:
                            break;
                    }
                    return _viewProvider.GetData(name);
                });
        }
        public object GetData(string name)
        {
            return _cache[name];
        }
    }
}
