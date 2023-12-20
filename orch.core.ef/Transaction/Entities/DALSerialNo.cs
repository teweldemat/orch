using orch.common;
using orch.core.model;

namespace orch.core.ef.Transaction.Entities
{
    public class DALSerialNo : SerialNoProps
    {
        public DALSerialNo()
        { }

        public DALSerialNo(SerialNoProps props)
        {
            this.MapFromBase(props);
        }
    }
}