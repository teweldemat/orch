using orch.common;
using orch.core.model;

namespace orch.core.ef.Transaction.Entities
{
    public class DALCommand : OCommandProps
    {
        public DALCommand()
        { }

        public DALCommand(OCommandProps props)
        {
            this.MapFromBase(props);
        }

        public DALUserInfo? User { get; set; }
        public DALOTransaction Tran { get; set; }
    }
}