namespace orch.wf.model.dto
{
    public class TaskStatePair<T> where T : WfStateData
    {
        public Guid Id { get; set; }
        public string TaskTypeKey { get; set; } = string.Empty;
        public string TaskTypeName { get; set; } = string.Empty;
        public OTask Task { get; set; } = null!;
        public T? StateData { get; set; }
    }
}
