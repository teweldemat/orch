using orch.core.model;

namespace orch.core.command.serial
{
    [CommandType(
        TYPE_ID,
        COMMAND_TYPE_KEY,
        "Update Serial Batch",
        initializer: typeof(UpdateSerialBatchCommandInitializer),
        handler: typeof(UpdateSerialBatchCommandHandler)
    )]
    public class UpdateSerialBatchCommand
    {
        public const string COMMAND_TYPE_KEY = "SYS_UPDATE_SERIAL_BATCH";
        public const string TYPE_ID = "b2f3c6d8-a4e5-4f67-9c12-d3e8f5a7b901";

        public Guid BatchId { get; set; }
        public string Description { get; set; }
        public int FromSerialNo { get; set; }
        public int ToSerialNo { get; set; }
        public int MaxUsed { get; set; }
    }

    public class UpdateSerialBatchCommandInitializer : CommandInitializerBase<UpdateSerialBatchCommand>
    {
        public UpdateSerialBatchCommandInitializer(IOHost host) : base(host)
        {
        }

        public override void Init()
        {
            if (_commandData.BatchId == Guid.Empty)
                throw new InvalidOperationException("BatchId must not be empty.");

            _commandData.Description = _commandData.Description?.Trim();
            if (string.IsNullOrWhiteSpace(_commandData.Description))
                throw new InvalidOperationException("Description must not be empty.");

            if (_commandData.ToSerialNo <= 0)
                throw new InvalidOperationException("ToSerialNo must be greater than 0.");
        }
    }

    public class UpdateSerialBatchCommandHandler : CommandHandlerBase<UpdateSerialBatchCommand>, ICommandHandler
    {
        public UpdateSerialBatchCommandHandler(TransactionServiceCollection services) : base(services)
        {
        }

        protected override void Authorize()
        {
            if (!_commandInfo.UserId.HasValue ||
                !_services.TranDb.IsPermitted(_commandInfo.UserId.Value, CoreModule.PERMISSION_MANAGE_SERIALS))
                throw new UnauthorizedAccessException("You are not authorized to manage serials.");
        }

        private SerialBatch _serialBatch;

        private SerialBatch SerialBatch
        {
            get
            {
                return _serialBatch ??= _services.TranDb.GetSerialBatch(_commandData.BatchId)
                                         ?? throw new InvalidOperationException("Serial batch with the provided ID does not exist.");
            }
        }

        public override string Summarize(out bool html)
        {
            html = false;
            return $"Updated serial batch {SerialBatch.Description}: description to '{_commandData.Description}' and to serial number to {_commandData.ToSerialNo}";
        }

        public override void Preprocess()
        {
            SerialBatch.FromSerialNo = _commandData.FromSerialNo;
            SerialBatch.ToSerialNo = _commandData.ToSerialNo;
            SerialBatch.MaxUsed = _commandData.MaxUsed;
            SerialBatch.Description = _commandData.Description;
        }

        protected override void Execute()
        {
            _services.TranDb.UpdateSerialBatch(_commandInfo, SerialBatch);
        }
    }
}