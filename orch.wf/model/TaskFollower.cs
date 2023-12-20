using orch.common;

namespace orch.wf.model
{
    public class TaskFollower : TaskFollowerProps
    {
        public TaskFollower() { }
        public TaskFollower(TaskFollowerProps props)
        => this.MapFromBase(props);
    }
    
}
