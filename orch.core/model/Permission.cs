using orch.common;

namespace orch.core.model
{
    public class Permission: PermissionProps
    {
        public Permission() { }
        public Permission(PermissionProps props)
            => this.MapFromBase(props);
    }
    public class RolePermissionDto
    {
        public List<string> PermissionNames { get; set; }
        public string RoleName { get; set; }
    }

}