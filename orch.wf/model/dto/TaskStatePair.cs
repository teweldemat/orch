namespace orch.wf.model.dto
{
    public class TaskStatePair<T> where T : WfStateData
    {
        public Guid Id { get; set; }
        public string TaskTypeKey { get; set; }
        public string TaskTypeName { get; set; }
        public OTask Task { get; set; }
        public T? StateData { get; set; }
    }
}
