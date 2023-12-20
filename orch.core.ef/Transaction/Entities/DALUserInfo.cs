using orch.common;
using orch.core.model;

namespace orch.core.ef.Transaction.Entities
{
    public class DALUserInfo : UserInfoProps
    {
        public DALUserInfo()
        { }

        public DALUserInfo(UserInfoProps props)
            => this.MapFromBase(props);

        public virtual ICollection<DALUserRole> Roles { get; set; }
    }
}