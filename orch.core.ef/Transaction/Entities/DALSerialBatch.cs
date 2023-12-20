using orch.common;
using orch.core.model;

namespace orch.core.ef.Transaction.Entities
{
    public class DALSerialBatch : SerialBatchProps
    {
        public DALSerialBatch()
        { }

        public DALSerialBatch(SerialBatchProps props)
        {
            this.MapFromBase(props);
        }
    }
}