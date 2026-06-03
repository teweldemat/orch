using orch.common;
using orch.core.model;

namespace orch.core.ef.Transaction.Entities
{
    public class DALRole : RoleProps
    {
        public DALRole()
        { }

        public DALRole(RoleProps props)
        {
            this.MapFromBase(props);
        }

        public ICollection<DALRolePermission> Permissions { get; set; } = new List<DALRolePermission>();
        public ICollection<DALUserRole> Users { get; set; } = new List<DALUserRole>();
    }
}