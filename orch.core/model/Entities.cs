using orch.common;

namespace orch.core.model
{
    public class OTransaction : OTransactionProps
    {
        public OTransaction() { }
        public OTransaction(OTransactionProps props)
            => this.MapFromBase(props);

        public virtual ICollection<OCommand> Commands { get; set; } = new List<OCommand>();

    }
}
