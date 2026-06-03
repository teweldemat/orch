using orch.core.model;

namespace orch.wf
{
    public abstract class WfStateData : ChangeProps
    {
        public Guid TaskId { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

    }
}
