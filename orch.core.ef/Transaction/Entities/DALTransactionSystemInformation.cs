using orch.common;
using orch.core.model;

namespace orch.core.ef.Transaction.Entities
{
    public class DALTransactionSystemInformation : TransactionSystemInformationProps
    {
        public DALTransactionSystemInformation()
        { }

        public DALTransactionSystemInformation(TransactionSystemInformationProps props)
        {
            this.MapFromBase(props);
        }
    }
}