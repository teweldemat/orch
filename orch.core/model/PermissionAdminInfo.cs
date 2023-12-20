using orch.common;

namespace orch.core.model
{
    public class PermissionAdminInfo: PermissionAdminInfoProps
    {
        public PermissionAdminInfo() { }
        public PermissionAdminInfo(PermissionAdminInfoProps props)
            => this.MapFromBase(props);
        public List<String> PermissionKeys { get; set; }
    }
    public abstract class PermissionAdminInfoProps
    {
        public Guid TranId;
        public String PermissionAdminKey { get; set; }
        public String PermssionAdminName { get; set; }
    }
    
}