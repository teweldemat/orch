using orch.core.model;

namespace orch.wf
{
    public abstract class WfStateData : ChangeProps
    {
        public Guid TaskId { get; set; }
        public string Reference { get; set; }
        public string Description { get; set; }

    }
}
