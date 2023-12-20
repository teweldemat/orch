using orch.core.model;
using orch.wf.model;
using orch.wf.model.dto;

namespace orch.wf
{
    public interface IWfService
    {
        void AssertAction(Guid wfTypeId, WfStateData wfStateData, UserInfo user, Guid actionTypeId, object action);
        void AssignWorkflow(OCommand command, WfStateData wfStateData);
        void ChangeState(OCommand command, Guid taskId, TaskChange taskState);
        IWfHandler GetWfHandler(Guid typeId);
        IList<WfService.WorkFlowAction> GetWorkflowAction(string type);
        IList<WfTypeInfoSummary> GetWorkflowTypes();
        bool IsActionApplicable(string taskType, Guid? taskId, Guid? userId, string actionType);
    }
}