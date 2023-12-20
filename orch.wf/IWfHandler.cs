using orch.core.model;
using orch.wf.model;

namespace orch.wf
{
    public interface IWfHandler
    {
        void OnBeforeWaitedWfChanged(OCommand command, Guid monitorTask, WfStateData monitoredTask, Guid actionTypeId, object action);
        void OnAfterWaitedWfChanged(OCommand command, Guid monitorTask, WfStateData monitoredTask, Guid actionTypeId, object action);
        RuleCheckResult IsActionTypeAvialable(Guid? thisTaskId, OTask task, WfStateData wfState, Guid actionTypeId);
        RuleCheckResult IsActionTypeAvialableForUser(Guid? thisTaskId, OTask task, WfStateData wfState, UserInfo user, Guid actionTypeId);
        RuleCheckResult IsActionAvialableForUser(Guid? thisTaskId, OTask task, WfStateData wfState, UserInfo user, Guid actionTypeId, object action);
    }
}
