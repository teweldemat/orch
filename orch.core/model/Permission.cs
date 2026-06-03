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
        public List<string> PermissionNames { get; set; } = new();
        public string RoleName { get; set; } = string.Empty;
    }

}