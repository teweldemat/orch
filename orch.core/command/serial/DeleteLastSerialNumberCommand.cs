using orch.core.model;

namespace orch.core.command.serial
{
    [CommandType(
        TYPE_ID,
        COMMAND_TYPE_KEY,
        "Delete Last Serial Number",
        initializer: typeof(DeleteLastSerialNumberCommandInitializer),
        handler: typeof(DeleteLastSerialNumberCommandHandler)
    )]
    public class DeleteLastSerialNumberCommand
    {
        public const string COMMAND_TYPE_KEY = "SYS_DELETE_LAST_SERIAL";
        public const string TYPE_ID = "9cf29c6f-4774-4b69-adbc-320ad148817f";

        public Guid BatchId { get; set; }
    }

    public class DeleteLastSerialNumberCommandInitializer : CommandInitializerBase<DeleteLastSerialNumberCommand>
    {
        public DeleteLastSerialNumberCommandInitializer(IOHost host) : base(host)
        {
        }

        public override void Init()
        {
            if (_commandData.BatchId == Guid.Empty)
                throw new InvalidOperationException("BatchId must not be empty.");
        }
    }

    public class DeleteLastSerialNumberCommandHandler : CommandHandlerBase<DeleteLastSerialNumberCommand>,
        ICommandHandler
    {
        public DeleteLastSerialNumberCommandHandler(TransactionServiceCollection services) : base(services)
        {
        }

        protected override void Authorize()
        {
            if (_commandInfo.UserId is not { } userId ||
                !_services.TranDb.IsPermitted(userId, CoreModule.PERMISSION_MANAGE_SERIALS))
            {
                throw new UnauthorizedAccessException("You are not authorized to manage serials.");
            }
        }

        private SerialNo? _lastSerialNo;

        private SerialNo? LastSerialNo
        {
            get { return _lastSerialNo ??= _services.TranDb.GetLastSerialNo(_commandData.BatchId); }
        }


        public override string Summarize(out bool html)
        {
            html = false;
            return $"Last serial number '{LastSerialNo?.Formatted}' in batch '{_commandData.BatchId}' deleted.";
        }

        public override void Preprocess()
        {
            if (LastSerialNo == null)
                throw new InvalidOperationException("No serial numbers have been used from this batch yet.");
        }

        protected override void Execute()
        {
            _services.TranDb.DeleteLastSerialNo(_commandInfo, _commandData.BatchId);
        }
    }
}