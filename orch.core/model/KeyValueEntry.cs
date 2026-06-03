using orch.common;

namespace orch.core.model
{
    public abstract class KeyValueEntryProps : ChangeProps
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;

    }

    public class KeyValueEntry : KeyValueEntryProps
    {
        public KeyValueEntry() { }

        public KeyValueEntry(KeyValueEntryProps props) => this.MapFromBase(props);
    }
}
