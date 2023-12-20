using orch.common;

namespace orch.core.model
{
    public class SerialType : SerialTypeProps
    {
        public SerialType() { }
        public SerialType(SerialTypeProps props)
            => this.MapFromBase(props);
    }

    public enum SerialNoFormattingType
    {
        DotNet,
        Formula
    }
    public abstract class SerialTypeProps : ChangeProps
    {
        public Guid Id { get; set; }
        public String Key { get; set; }
        public String Name { get; set; }
        public String FormatString { get; set; }
        public SerialNoFormattingType FormatType { get; set; }
        public String FormatSerialNo(int sn)
        {
            return String.Format(FormatString, sn);

        }
        public string AuthorizationLevel { get; set; }
    }
    public class SerialBatch : SerialBatchProps
    {
        public SerialBatch() { }
        public SerialBatch(SerialBatchProps props)
            => this.MapFromBase(props);
    }
    public abstract class SerialBatchProps : ChangeProps
    {
        public Guid Id { get; set; }
        public Guid SerialTypeId { get; set; }
        public String Description { get; set; }
        public int FromSerialNo { get; set; }
        public int ToSerialNo { get; set; }
        public int MaxUsed { get; set; }
    }
    public class SerialNo : SerialNoProps
    {
        public SerialNo() { }
        public SerialNo(SerialNoProps props)
            => this.MapFromBase(props);
    }
    public class SerialNoProps : ChangeProps
    {
        public Guid BatchId { get; set; }
        public int Sn { get; set; }
        public String Formatted { get; set; }
        public bool IsVoid { get; set; }

    }
}
