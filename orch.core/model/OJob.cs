using orch.common;

namespace orch.core.model
{
    public class OJob : OJobProps
    {
        public Guid? SystemID { get; set; }

        public OJob() { }
        public OJob(OJobProps props)
                => this.MapFromBase(props);
    }


}