using orch.common;
using orch.core.model;

namespace orch.core.ef.Transaction.Entities
{
    public class DALPermission : PermissionProps
    {
        public DALPermission()
        { }

        public DALPermission(PermissionProps props)
        {
            this.MapFromBase(props);
        }

        public virtual ICollection<DALRolePermission> Roles { get; set; }
    }
}