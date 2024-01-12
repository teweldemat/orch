using orch.common;
using orch.core.model;

namespace orch.core.ef.Transaction.Entities
{
    public class DALOJob : OJobProps
    {
        public DALOJob()
        { }

        public DALOJob(OJobProps props)
        {
            this.MapFromBase(props);
        }
        public DALUserInfo? User { get; set; }
    }
}
