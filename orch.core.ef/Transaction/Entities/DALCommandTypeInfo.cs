namespace orch.core.ef.Transaction.Entities
{
    public class DALCommandTypeInfo
    {
        public long UpdateTime { get; set; }
        public Guid CommandTypeId { get; set; }
        public String CommandKey { get; set; } = "";
        public String CommandName { get; set; } = "";
        
    }
}