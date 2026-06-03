using FuncScript;
using FuncScript.Core;
using FuncScript.Model;
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
        FsFormula
    }
    public abstract class SerialTypeProps : ChangeProps
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string FormatString { get; set; } = string.Empty;
        public SerialNoFormattingType FormatType { get; set; }
        public string? AuthorizationLevel { get; set; }

        public string FormatSerialNo(int sn, KeyValueCollection? provider = null)
        {
            if (string.IsNullOrWhiteSpace(FormatString))
                return sn.ToString();

            switch (FormatType)
            {
                case SerialNoFormattingType.DotNet:
                    return string.Format(FormatString, sn);
                case SerialNoFormattingType.FsFormula:
                {
                    object result;
                    try
                    {
                        result = Engine.Evaluate(new KvcProvider(new ObjectKvc(new
                            {
                                serialNo = sn
                            }),
                            provider
                        ), FormatString);
                    }
                    catch (Exception ex)
                    {
                        throw new ApplicationException(
                            $"An error occurred while evaluating serial type '{Key}' with formatting type '{FormatType}'",
                            ex);
                    }

                    if (result is null)
                        throw new ApplicationException(
                            $"Evaluation of serial type '{Key}' with formatting type '{FormatType}' did not return any value");

                    if (result is not string formatted)
                        throw new ApplicationException(
                            $"Evaluation of serial type '{Key}' with formatting type '{FormatType}' returned non-string value '{result}' of type '{result.GetType()}'");

                    return formatted;
                }
                default:
                    throw new NotSupportedException(
                        $"Serial type '{Key}' has unsupported formatting type '{FormatType}'");
            }
        }
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
        public string Description { get; set; } = string.Empty;
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
        public string Formatted { get; set; } = string.Empty;
        public bool IsVoid { get; set; }

    }
}
