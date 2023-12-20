using orch.common;
using orch.core.model;

namespace orch.core.ef.Transaction.Entities
{
    public class DALPermissionAdminInfo : PermissionAdminInfoProps
    {
        public DALPermissionAdminInfo()
        { }

        public DALPermissionAdminInfo(PermissionAdminInfoProps props)
        {
            this.MapFromBase(props);
        }
    }
}