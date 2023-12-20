namespace orch.core.ef.Transaction.Entities
{
    public class DALRolePermission
    {
        public DALRolePermission()
        { }

        public Guid RoleId { get; set; }
        public Guid PermissionId { get; set; }
        public int Order { get; set; }
        public DALRole Role { get; set; }
        public DALPermission Permission { get; set; }
    }
}