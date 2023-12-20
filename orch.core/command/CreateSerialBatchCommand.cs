using orch.core.model;

namespace orch.core.command
{
    [CommandType(
        TYPE_ID,
        COMMAND_TYPE_KEY,
        "Set Serial Batch",
        initializer: typeof(CreateSerialBatchInitialier),
        handler: typeof(CreateSerialBatchCommandHandler)
    )]
    public class CreateSerialBatchCommand
    {
        public const string COMMAND_TYPE_KEY = "SYS_SET_SERIAL_BATCH";
        public const string TYPE_ID = "1f3f9b3c-11a5-40cc-aebc-42fe7a8aa8f6";
        public SerialType SerialType { get; set; }
        public SerialBatch SerialBatch { get; set; }
    }

    public class CreateSerialBatchInitialier : CommandInitializerBase<CreateSerialBatchCommand>
    {
        public CreateSerialBatchInitialier(IOHost host) : base(host)
        {
        }

        public override void Init()
        {
            if (_commandData.SerialBatch == null && _commandData.SerialType == null)
            {
                throw new InvalidOperationException("Both SerialBatch and SerialType are null.");
            }

            if (_commandData.SerialType != null)
            {
                _commandData.SerialType.Id = Host.NextGuid();
                _commandData.SerialBatch.SerialTypeId = _commandData.SerialType.Id;
            }

            if (_commandData.SerialBatch != null && _commandData.SerialBatch.Id == Guid.Empty)
            {
                _commandData.SerialBatch.Id = Host.NextGuid();
            }
        }
    }


    public class CreateSerialBatchCommandHandler : CommandHandlerBase<CreateSerialBatchCommand>, ICommandHandler
    {
        public CreateSerialBatchCommandHandler(TransactionServiceCollection services) : base(services)
        {
        }
        public override string Summarize(out bool html)
        {
            html = false;

            string serialTypeSummary = _commandData.SerialType != null ? $"Serial type {_commandData.SerialType.Key} created" : string.Empty;
            string serialBatchSummary = _commandData.SerialBatch != null ? $"Serial batch {_commandData.SerialBatch.Description} created" : string.Empty;

            return $"{serialTypeSummary}{(string.IsNullOrEmpty(serialTypeSummary) || string.IsNullOrEmpty(serialBatchSummary) ? "" : ", ")}{serialBatchSummary}";
        }


        protected override void Authorize()
        {
            if (!_services.TranDb.IsPermitted(_commandInfo.UserId.Value, CoreModule.PERMISSION_MANAGE_SERIALS))
                throw new UnauthorizedAccessException();
        }

        protected override void Execute()
        {
            if (_commandData.SerialType != null)
            {
                _services.TranDb.CreateSerialType(_commandInfo, _commandData.SerialType);
            }
            if (_commandData.SerialBatch != null)
            {
                _services.TranDb.CreateSerialBatch(_commandInfo, _commandData.SerialBatch);
            }
        }
    }

}