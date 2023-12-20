using orch.common;

namespace orch.core.model
{
    public class OrganizationData : OrganizationDataProps
    {
        public OrganizationData() { }
        public OrganizationData(OrganizationDataProps props)
        {
            this.MapFromBase(props);
        }

    }
}
