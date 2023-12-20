using orch.common;

namespace orch.wf.model
{
    public class TaskAssignee : TaskAssigneeProps
    {
        public TaskAssignee() { }
        public TaskAssignee(TaskAssigneeProps props)
        => this.MapFromBase(props);
    }
    
}
