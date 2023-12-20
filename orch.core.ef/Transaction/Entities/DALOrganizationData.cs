using orch.common;
using orch.core.model;

namespace orch.core.ef.Transaction.Entities
{
    public class DALOrganizationData : OrganizationDataProps
    {
        public DALOrganizationData()
        { }

        public DALOrganizationData(OrganizationDataProps props)
        {
            this.MapFromBase(props);
        }
    }
}