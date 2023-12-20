using orch.common;
using orch.core.model;

namespace orch.core.ef.Transaction.Entities
{
    public class DALSerialType : SerialTypeProps
    {
        public DALSerialType()
        { }

        public DALSerialType(SerialTypeProps props)
        {
            this.MapFromBase(props);
        }
    }
}