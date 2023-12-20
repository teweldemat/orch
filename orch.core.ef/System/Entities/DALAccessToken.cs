using orch.common;
using orch.core.model;

namespace orch.core.ef.System.Entities
{
    public class DALAccessToken : AccessTokenProps
    {
        public DALAccessToken()
        { }

        public DALAccessToken(AccessTokenProps props)
            => this.MapFromBase(props);
    }
}