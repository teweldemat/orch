using orch.core.model;

namespace orch.core.command;

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
    public const string TYPE_ID = "7f0bfee3-b492-43e5-8bf7-a978caecbaec";
    
    public Guid SerialBatchId { get; set; }
    public int? MaxUsed { get; set; }
    
    bool Any()
    {
        return MaxUsed.HasValue;
    }
    
    
    public class UpdateSerialBatchCommandInitializer : CommandInitializerBase<UpdateSerialBatchCommand>
    {
        public UpdateSerialBatchCommandInitializer(IOHost host) : base(host)
        {
        }
        
        public override void Init()
        {
            if (_commandData.SerialBatchId == Guid.Empty)
                throw new InvalidOperationException("SerialBatchId must not be empty.");
            
            if (!_commandData.Any())
                throw new InvalidOperationException("Command body is empty.");
            
            if (_commandData.MaxUsed is {} and < 0)
                throw new InvalidOperationException("MaxUsed must be greater than or equal to 0.");
        }
    }
    
    public class UpdateSerialBatchCommandHandler : CommandHandlerBase<UpdateSerialBatchCommand>, ICommandHandler
    {
        public UpdateSerialBatchCommandHandler(TransactionServiceCollection services) : base(services)
        {
        }
        
        protected override void Authorize()
        {
            if (_commandInfo.UserId is not {} userId || !_services.TranDb.IsPermitted(userId, CoreModule.PERMISSION_MANAGE_SERIALS))
                throw new UnauthorizedAccessException("You are not authorized to manage serials.");
        }
        
        public override string Summarize(out bool html)
        {
            html = false;
            return $"Serial type '{SerialBatch.Id}' updated";
        }

        private SerialBatch _SerialBatch;

        private SerialBatch SerialBatch
        {
            get { return _SerialBatch ??= _services.TranDb.GetSerialBatch(_commandData.SerialBatchId) 
                                         ?? throw new InvalidOperationException("Serial type with the provided ID does not exist."); }
        }
        
        public override void Preprocess()
        {
            if (_commandData.MaxUsed.HasValue)
                SerialBatch.MaxUsed = _commandData.MaxUsed.Value;
        }

        protected override void Execute()
        {
           _services.TranDb.UpdateSerialBatch(_commandInfo, SerialBatch);
        }
    }
}