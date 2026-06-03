using orch.core;
using orch.core.model;
using orch.wf.model;

namespace orch.wf
{
    public class RuleCheckResult
    {
        public bool Yes { get; set; }
        public string? Reason { get; set; }
        public RuleCheckResult()
        {
        }
        public RuleCheckResult(bool res)
        {
            Yes = res;
            Reason = null;
        }

        public RuleCheckResult(string res)
        {
            Yes = false;
            Reason = res;
        }
    }

    public interface IWfAction
    {
        IList<string> RequiredPermissions(OCommand? command, WfStateData? stateData);
        bool Assignable { get; }
    }


    public abstract class WfActionCommandHandler<W, T> : CommandHandlerBase<T>, IWfAction where W : WfStateData
    {
        public virtual bool Assignable => true;
        WfServiceCollection Services => (WfServiceCollection)_services;
        protected WfActionCommandHandler(WfServiceCollection services) : base(services)
        {
        }
        protected abstract void ExecuteAction();
        protected abstract void PreprocessAction();
        protected virtual void PostExecuteAction() { }
        protected abstract W? StateData { get; }
        private HashSet<Guid> NotificationIds { get; } = new();
        public abstract IList<string> RequiredPermissions(OCommand? command, WfStateData? stateData);
        protected virtual bool AssertAction => _mainCommand;

        private Guid ActionTypeId =>
            OTransactionService.GetTypeIdByType(typeof(T))?.TypeId
            ?? throw new InvalidOperationException($"Command type {typeof(T)} is not registered.");

        public sealed override void Preprocess()
        {
            if (AssertAction)
            {
                var stateBefore = StateData;

                if (_commandInfo.UserId.HasValue)
                {
                    var user = _services.TranDb.GetUserInfo(_commandInfo.UserId.Value);
                    var wfType = WfModule.GetWfTypeInfo(typeof(W))
                        ?? throw new InvalidOperationException($"Workflow type for {typeof(W)} not found");
                    Services.WfService.AssertAction(wfType.Id, stateBefore, user, ActionTypeId, _commandData);
                }
            }

            PreprocessAction();
        }
        class MonitorInfo
        {
            public OTask task = null!;
            public IWfHandler h = null!;
        }

        protected sealed override void Execute()
        {
            IEnumerable<MonitorInfo>? monitors = null;

            if (StateData != null)
            {
                monitors = Services.WfDb.GetTaskMonitors(StateData.TaskId)
                    .Select(x =>
                    {
                        var monitorTask = Services.WfDb.GetTask(x);
                        if (monitorTask == null)
                            return null;
                        var handler = Services.WfService.GetWfHandler(monitorTask.TaskTypeId);
                        return handler == null
                            ? null
                            : new MonitorInfo
                            {
                                task = monitorTask,
                                h = handler
                            };
                    })
                    .Where(x => x != null)
                    .Select(x => x!);

                var stateData = StateData;
                foreach (var m in monitors)
                {
                    m.h.OnBeforeWaitedWfChanged(
                        _commandInfo,
                        m.task.Id,
                        stateData,
                        ActionTypeId,
                        _commandData);
                }
            }

            ExecuteAction();

            if (monitors != null && StateData != null)
            {
                var stateData = StateData;
                foreach (var m in monitors)
                {
                    m.h.OnAfterWaitedWfChanged(
                        _commandInfo,
                        m.task.Id,
                        stateData,
                        ActionTypeId,
                        _commandData);
                }
            }

            AssignWorkflow();

            PostExecuteAction();
        }

        public sealed override void PostExecute()
        {
            if (NotificationIds.Any())
                Services.WfNotificationService.SendNotifications(NotificationIds.ToArray());
        }

        protected internal virtual void AssignWorkflow()
        {
            if (StateData == null)
                return;
            Services.WfService.AssignWorkflow(_commandInfo, StateData);
        }

        protected sealed override void Authorize()
        {
            var wf = StateData;
            foreach (var p in RequiredPermissions(_commandInfo, wf))
            {
                var permission = _services.TranDb.GetPermission(p)
                    ?? throw new InvalidOperationException($"Permission '{p}' not found.");

                if (!_commandInfo.UserId.HasValue)
                    throw new UnauthorizedAccessException("User is not authorized. Missing user id on command.");

                if (!_services.TranDb.IsPermitted(_commandInfo.UserId.Value, permission.Id))
                    throw new UnauthorizedAccessException($"User is not authorized. Missing permission: {p}");
            }
        }

        protected void SendNotification(Guid id, string message, params Guid[] users)
        {
            if (StateData == null)
                throw new InvalidOperationException("State data is required to send a workflow notification.");

            Services.WfDb.SendNotification(
                  _commandInfo,
                  new WfNotification()
                  {
                      Id = id,
                      TaskId = StateData.TaskId,
                      Message = message,
                      Time = _commandInfo.Time
                  },
                  users);

            NotificationIds.Add(id);
        }
    }
}
