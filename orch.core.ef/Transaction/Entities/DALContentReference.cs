using orch.common;
using orch.core.model;

namespace orch.core.ef.Transaction.Entities
{
    public class DALContentReference : ContentReferenceProps
    {
        public DALContentReference()
        { }

        public DALContentReference(ContentReferenceProps props)
        {
            this.MapFromBase(props);
        }
    }
}