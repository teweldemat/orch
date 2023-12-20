using orch.common;

namespace orch.core.model
{
    public class UserInfo : UserInfoProps
    {
        public UserInfo() { }
        public UserInfo(UserInfoProps props)
            => this.MapFromBase(props);
        public List<Guid> Roles { get; set; }

    }

    public class UserHistory : UserHistoryProps
    {
        public UserHistory() { }
        public UserHistory(UserHistoryProps props)
            => this.MapFromBase(props);
    }


}