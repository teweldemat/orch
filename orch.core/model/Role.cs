using orch.common;

namespace orch.core.model
{
    public class Role:RoleProps
    {
        public Role() { }
        public Role(RoleProps props)
            => this.MapFromBase(props);
        public IList<Guid> Permissions { get; set; } 
    }
    

}