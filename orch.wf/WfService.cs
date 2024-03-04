using funcscript;
using orch.core;
using orch.core.model;
using orch.wf.model;
using orch.wf.model.dto;

namespace orch.wf
{
    [OView("wf")]
    public class WfService : IWfService
    {
        private readonly OTransactionService _tranService;
        private readonly ITransactionDatabase _tranDb;
        private readonly IWfDatabase _wfDb;

        public WfService(OTransactionService tranService, ITransactionDatabase tranDb, IWfDatabase wfDb)
        {
            this._tranService = tranService;
            this._tranDb = tranDb;
            this._wfDb = wfDb;
        }

        public void AssertAction(Guid wfTypeId, WfStateData wfStateData, UserInfo user, Guid actionTypeId, object action)
        {
            var task = wfStateData == null ? null : _wfDb.GetTask(wfStateData.TaskId);
            var wfHandler = GetWfHandler(wfTypeId);
            var monitors = wfStateData == null ? null : _wfDb.GetTaskMonitors(task.Id)
                .Select(x =>
                {
                    var mon = _wfDb.GetTask(x);
                    return new { task = mon, handler = GetWfHandler(mon.TaskTypeId) };
                }).Where(x => x.handler != null);

            //check action type applicability
            if (monitors != null)
                foreach (var mon in monitors)
                {

                    var res = mon.handler.IsActionTypeAvialable(mon.task.Id, task, wfStateData, actionTypeId);
                    if (!res.Yes)
                        throw new InvalidOperationException($"Action type '{OTransactionService.GetTypeInfoById(actionTypeId).TypeName}' is not applicable at this state.{res.Reason}");
                }
            if (wfHandler != null)
            {
                var res = wfHandler.IsActionTypeAvialable(wfStateData?.TaskId, task, wfStateData, actionTypeId);
                if (!res.Yes)
                    throw new InvalidOperationException($"Action type '{OTransactionService.GetTypeInfoById(actionTypeId).TypeName}' is not applicable at this state. {res.Reason}");
            }

            //check user access to the action type
            if (monitors != null)
                foreach (var mon in monitors)
                {
                    var res = mon.handler.IsActionTypeAvialableForUser(mon.task.Id, task, wfStateData, user, actionTypeId);
                    if (!res.Yes)
                        throw new UnauthorizedAccessException($"You are not allowed to perform action '{OTransactionService.GetTypeInfoById(actionTypeId).TypeName}'. {res.Reason}");
                }
            if (wfHandler != null)
            {
                var res = wfHandler.IsActionTypeAvialableForUser(wfStateData?.TaskId, task, wfStateData, user, actionTypeId);
                if (!res.Yes)
                    throw new UnauthorizedAccessException($"You are not allowed to perform action '{OTransactionService.GetTypeInfoById(actionTypeId).TypeName}'. {res.Reason}");
            }


            //check user permission to periform the  action with the specific acation data
            if (monitors != null)
                foreach (var mon in monitors)
                {
                    var res = mon.handler.IsActionAvialableForUser(mon.task.Id, task, wfStateData, user, actionTypeId, action);
                    if (!res.Yes)
                        throw new UnauthorizedAccessException($"You are not allowed to perform action '{OTransactionService.GetTypeInfoById(actionTypeId).TypeName}'. {res.Reason}");
                }
            if (wfHandler != null)
            {
                var res = wfHandler.IsActionAvialableForUser(wfStateData?.TaskId, task, wfStateData, user, actionTypeId, action);
                if (!res.Yes)
                    throw new UnauthorizedAccessException($"You are not allowed to perform action '{OTransactionService.GetTypeInfoById(actionTypeId).TypeName}'. {res.Reason}");
            }
        }
        public void AssignWorkflow(OCommand command, WfStateData wfStateData)
        {
            if (wfStateData == null)
                throw new ArgumentNullException(nameof(wfStateData), "State data cannot be null during workflow assignment.");

            var filterdUsers = new HashSet<Guid>();

            var wfType = WfModule.GetWfTypeInfo(wfStateData.GetType())
                ?? throw new InvalidOperationException($"Workflow information for tyoe {wfStateData.GetType()} not found");

            var task = _wfDb.GetTask(wfStateData.TaskId);
            var wfHandler = GetWfHandler(task.TaskTypeId);

            var monitors = _wfDb.GetTaskMonitors(task.Id)
                .Select(x =>
                {
                    var mon = _wfDb.GetTask(x);
                    return new { task = mon, handler = GetWfHandler(mon.TaskTypeId) };
                }).Where(x => x.handler != null);

            foreach (var a in wfType.AllActions)
            {
                var commandHandler = _tranService.GetHandler(OTransactionService.GetTypeInfoById(a).TypeId)
                    ?? throw new InvalidOperationException($"Tranasction handler information for for action {a} not found");

                if (commandHandler is not IWfAction action)
                    throw new InvalidOperationException($"Tranasction handler {commandHandler.GetType()} doesn't implement IWfAction interface");

                if (!action.Assignable)
                    continue;

                //if handlers any of the monitor workfows block the action return
                foreach (var mon in monitors)
                {
                    if (!mon.handler.IsActionTypeAvialable(mon.task.Id, task, wfStateData, a).Yes)
                        return;
                }
                //check the handler for the workflow
                if (wfHandler != null)
                {
                    if (!wfHandler.IsActionTypeAvialable(wfStateData.TaskId, task, wfStateData, a).Yes)
                        continue;
                }
                var perms = action.RequiredPermissions(command, wfStateData)
                    .Select(x =>
                    {
                        var perm = _tranDb.GetPermission(x);
                        return perm == null
                            ? throw new InvalidOperationException($"Permission '{x}' returned by '{action.GetType()}' as required is not valid.")
                            : perm.Id;
                    }
                    ).ToList();

                var users = _tranDb.GetUserWithPermissions(perms);

                if (DefaultFsDataProvider.Trace)
                    DefaultFsDataProvider.WriteTraceLine("Filtering applicable users");

                users = users.Where(x =>
                    {
                        var user = _tranDb.GetUserInfo(x);
                        //if handlers any of the monitor workfows block the action return
                        foreach (var mon in monitors)
                        {
                            if (!mon.handler.IsActionTypeAvialableForUser(mon.task.Id, task, wfStateData, user, a).Yes)
                                return false;
                        }
                        if (wfHandler != null)
                        {
                            if (!wfHandler.IsActionTypeAvialableForUser(wfStateData.TaskId, task, wfStateData, user, a).Yes)
                                return false;
                        }
                        return true;
                    }).ToList();

                foreach (var u in users)
                    if (!filterdUsers.Contains(u))
                        filterdUsers.Add(u);
            }
            var alreadyAssigned = _wfDb.GetAssignedUsers(wfStateData.TaskId);
            _wfDb.AssignUsers(command, wfStateData.TaskId, filterdUsers.Where(x => !alreadyAssigned.Contains(x)).ToList()); ;
            _wfDb.UnassignUsers(command, wfStateData.TaskId, alreadyAssigned.Where(x => !filterdUsers.Contains(x)).ToList()); ;
        }
        public void ChangeState(OCommand command, Guid taskId, TaskChange taskState)
        {
            _wfDb.ChangeState(command, taskId, taskState);
        }
        public void CreateTask<T>(OCommand command, OTask task, T data, TaskNote note) where T : WfStateData
        {
            _wfDb.CreateTask<T>(command, task, data, note);
        }
        public IWfHandler GetWfHandler(Guid typeId)
        {
            var typeInfo = WfModule.GetWfTypeInfo(typeId);
            if (typeInfo != null && typeInfo.handler != null)
            {
                var pars = typeInfo.ConstructorParameters.Select(x =>
                {
                    if (x.IsAssignableFrom(typeof(OTransactionService)))
                        return this;
                    return _tranService.Services.GetService(x);
                }).ToArray();
                return Activator.CreateInstance(typeInfo.handler, pars) as IWfHandler;
            }
            return null;
        }

        public class WorkFlowAction
        {
            public Guid Id;
            public String Key;
            public String Description;
        }

        [OViewFunction]
        public IList<WorkFlowAction>? GetWorkflowAction(string type)
        {
            var typeInfo = WfModule.GetWfTypeInfo(type);
            if (typeInfo == null)
                return null;

            return typeInfo.AllActions.Select(actionId =>
            {
                var actionTypeInfo = OTransactionService.GetTypeInfoById(actionId);
                return new WorkFlowAction
                {
                    Id = actionTypeInfo.TypeId,
                    Key = actionTypeInfo.Key,
                    Description = actionTypeInfo.TypeName
                };
            }).ToList();
        }

        [OViewFunction(permissions: new string[] { CoreModule.PERMISSION_SYSTEM_ROOT })]
        public WfTypeInfoSummary? GetWorkflowTypeByKey(string key)
        {
            var wfTypeInfo = WfModule.GetWfTypeInfo(key);
            if (wfTypeInfo == null)
                return null;
            return new WfTypeInfoSummary(wfTypeInfo);
        }

        [OViewFunction(permissions: new string[] { CoreModule.PERMISSION_SYSTEM_ROOT })]
        public IList<WfTypeInfoSummary> GetWorkflowTypes()
        {
            return WfModule.GetWorkflowTypes().Select(typeInfo => new WfTypeInfoSummary(typeInfo)).ToList();
        }

        public class TaskTypeInformation
        {
            public Guid Id;
            public String Key;
            public String TypeName;
        }
        [OViewFunction]
        public TaskTypeInformation GetTaskTypeInfo(Guid taskTypeId)
        {
            var t = WfModule.GetWfTypeInfo(taskTypeId);
            return new TaskTypeInformation
            {
                Id = t.Id,
                Key = t.Key,
                TypeName = t.Name
            };
        }

        internal record Monitor(OTask Task, IWfHandler Handler);

        [OViewFunction]
        public bool IsActionApplicable(String taskType, Guid? taskId, Guid? userId, String actionType)
        {
            var user = userId == null ? null : _tranDb.GetUserInfo(userId.Value);

            var task = taskId == null ? null : _wfDb.GetTask(taskId.Value);
            var actionInfo = OTransactionService.GetTypeIdByKey(actionType);
            if (actionInfo == null)
                throw new InvalidOperationException($"Action type {actionType} is invalid");
            var type = WfModule.GetWfTypeInfo(taskType);
            if (type == null)
                throw new InvalidOperationException($"Task type {taskType} is invalid");

            var wfStateData = task == null ? (WfStateData)null : _wfDb.GetTaskData(type.Type, taskId.Value) as WfStateData;
            List<Monitor> monitors = null;
            var wfHandler = GetWfHandler(type.Id);
            if (task != null)
            {
                monitors = _wfDb.GetTaskMonitors(task.Id)
                    .Select(x =>
                    {
                        var mon = _wfDb.GetTask(x);
                        return new Monitor(mon, GetWfHandler(mon.TaskTypeId));
                    }).Where(x => x.Handler != null).ToList();

                //check action type applicability
                foreach (var mon in monitors)
                {
                    var res = mon.Handler.IsActionTypeAvialable(mon.Task.Id, mon.Task, wfStateData, actionInfo.TypeId);
                    if (!res.Yes)
                        return false;
                }
            }
            if (wfHandler != null)
            {
                var res = wfHandler.IsActionTypeAvialable(wfStateData == null ? null : wfStateData.TaskId, task, wfStateData, actionInfo.TypeId);
                if (!res.Yes)
                    return false;
            }
            if (user != null)
            {
                if (_tranDb.GetRootUser().Id != user.Id)
                {
                    ICommandHandler h = _tranService.GetHandler(actionInfo.TypeId);
                    if (h != null)
                    {
                        if (h is IWfAction)
                        {
                            var perms = this._tranDb.GetUserPermissions(userId.Value);
                            var required = ((IWfAction)h).RequiredPermissions(null, task == null ? null : (WfStateData)_wfDb.GetTaskData(type.Type, task.Id))
                                .Select(x => _tranDb.GetPermission(x).Id);
                            foreach (var r in required)
                            {
                                if (!perms.Any(x => x == r))
                                    return false;
                            }
                        }
                    }
                }
                if (task != null)
                {
                    //check user access to the action type
                    foreach (var mon in monitors)
                    {
                        var res = mon.Handler.IsActionTypeAvialableForUser(mon.Task.Id, mon.Task, wfStateData, user, actionInfo.TypeId);
                        if (!res.Yes)
                            return false;
                    }
                    if (wfHandler != null)
                    {
                        var res = wfHandler.IsActionTypeAvialableForUser(wfStateData == null ? null : wfStateData.TaskId, task, wfStateData, user, actionInfo.TypeId);
                        if (!res.Yes)
                            return false;
                    }
                }
            }
            return true;
        }
    }

}
