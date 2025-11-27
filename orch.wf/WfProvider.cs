using FuncScript;
using FuncScript.Core;
using FuncScript.Model;
using orch.core;
using orch.core.model;
using orch.wf.model;

namespace orch.wf
{
    public class WfProvider : FuncScript.Model.KeyValueCollection
    {
        DefaultFsDataProvider global = new DefaultFsDataProvider();
        common.CachedObject<string, object> _cache;
        WfServiceCollection _services;
        OTask _task;
        WfStateData _wfState;
        UserInfo _checkUser;
        ViewQueryProvider _viewProvider;
        private KeyValueCollection _parentProvider;

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

        public KeyValueCollection ParentProvider => _parentProvider;

        public bool IsDefined(string key,bool hierarchy=false)
        {
            var here= new string[] { "user", "task", "wfstate", "action" }.Contains(key.ToLower());
            if (here)
                return true;
            return _viewProvider.IsDefined(key, hierarchy);
        }
        public IList<KeyValuePair<string, object>> GetAll()
        {
            return (new string[] { "user", "task", "wfstate", "action" }
                .Select(name => KeyValuePair.Create(name, this.Get(name))))
                .Concat(_viewProvider.GetAll())
                .ToList();
        }
        

        public IList<string> GetAllKeys()
        {
            return new string[] { "user", "task", "wfstate", "action" }
                .Concat(_viewProvider.GetAllKeys())
                .ToList();;
        }
    }
}
