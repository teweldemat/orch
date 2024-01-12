using orch.common;

namespace orch.core.model
{
    public class OJob : OJobProps
    {
        public OJob() { }
        public OJob(OJobProps props)
                => this.MapFromBase(props);
    }


}