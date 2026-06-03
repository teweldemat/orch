namespace orch.core.ef.Transaction.Entities
{
    public class DALUserRole
    {
        public DALUserRole()
        { }

        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public int Order { get; set; }
        public DALUserInfo User { get; set; } = null!;
        public DALRole Role { get; set; } = null!;
    }
}