using orch.core.model;

namespace orch.core.ef.Transaction.DTOs
{
    public class AccessTokenInfo
    {
        public bool Valid { get; set; }
        public AccessToken? Info { get; set; }
        public AccessTokenInfo(bool valid, AccessToken? info) => (Valid, Info) = (valid, info);
    }
}