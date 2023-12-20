using orch.common;

namespace orch.core.model
{
    public class TransactionSystemInformation : TransactionSystemInformationProps
    {
        public TransactionSystemInformation() { }
        public TransactionSystemInformation(TransactionSystemInformationProps props)
            => this.MapFromBase(props);
    }
    public class TransactionSystemInformationProps : ChangeProps
    {
        public Guid SystemId { get; set; }
        public Guid? HeadTranId { get; set; }
        public long? LastTranTime { get; set; }
    }

}