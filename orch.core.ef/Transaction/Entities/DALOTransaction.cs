using orch.common;
using orch.core.model;

namespace orch.core.ef.Transaction.Entities
{
    public class DALOTransaction : OTransactionProps
    {
        public DALOTransaction()
        {
        }

        public DALOTransaction(OTransactionProps props)
        {
            this.MapFromBase(props);
        }

        public DALUserInfo? User { get; set; }
        public virtual ICollection<DALCommand> Commands { get; set; }
    }
}