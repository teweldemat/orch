namespace orch.ef.workflow.entities
{
    public class DALTaskData
    {
        public Guid TaskId { get; set; }
        public String? Data { get; set; }
        public DALOTask Task { get; set; }
    }
}
