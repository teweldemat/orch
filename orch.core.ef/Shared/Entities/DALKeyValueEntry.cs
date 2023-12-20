using orch.common;
using orch.core.model;

namespace orch.core.ef.Shared.Entities
{
    public class DALKeyValueEntry : KeyValueEntryProps
    {
        public DALKeyValueEntry()
        { }

        public DALKeyValueEntry(KeyValueEntryProps props) => this.MapFromBase(props);
    }
}