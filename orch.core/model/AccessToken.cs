using orch.common;

namespace orch.core.model
{
    public class AccessToken : AccessTokenProps
    {
        public AccessToken() { }
        public AccessToken(AccessTokenProps props)
            => this.MapFromBase(props);
        public UserInfo UserInfo { get; set; } = null!;
    }


}