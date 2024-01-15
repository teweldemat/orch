using orch.common;
using orch.core.model;

namespace orch.core.ef.Logging.Entities
{
    internal class DALEventLog : EventLogProps
    {
        public DALEventLog() { }

        public DALEventLog(EventLogProps props)
        {
            this.MapFromBase(props);
        }
    }
}
