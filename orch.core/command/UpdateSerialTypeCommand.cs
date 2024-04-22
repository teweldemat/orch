using orch.core.model;

namespace orch.core.command;

[CommandType(
    TYPE_ID,
    COMMAND_TYPE_KEY,
    "Update Serial Type",
    initializer: typeof(UpdateSerialTypeCommandInitializer),
    handler: typeof(UpdateSerialTypeCommandHandler)
)]
public class UpdateSerialTypeCommand
{
    
    public const string COMMAND_TYPE_KEY = "SYS_UPDATE_SERIAL_TYPE";
    public const string TYPE_ID = "35a97028-746d-4c45-986a-021973ca5fa8";
    
    public Guid SerialTypeId { get; set; }
    public string? AuthorizationLevel { get; set; }
    
    
    public class UpdateSerialTypeCommandInitializer : CommandInitializerBase<UpdateSerialTypeCommand>
    {
        public UpdateSerialTypeCommandInitializer(IOHost host) : base(host)
        {
        }
        
        public override void Init()
        {
            if (_commandData.SerialTypeId == Guid.Empty)
                throw new InvalidOperationException("SerialTypeId must not be empty.");
            
            _commandData.AuthorizationLevel = _commandData.AuthorizationLevel?.Trim();
            if (_commandData.AuthorizationLevel != null && string.IsNullOrWhiteSpace(_commandData.AuthorizationLevel))
                throw new InvalidOperationException("'AuthorizationLevel' must not be empty when provided.");
            
            if (_commandData.AuthorizationLevel is null)
                throw new InvalidOperationException("Command body is empty.");
        }
    }
    
    public class UpdateSerialTypeCommandHandler : CommandHandlerBase<UpdateSerialTypeCommand>, ICommandHandler
    {
        public UpdateSerialTypeCommandHandler(TransactionServiceCollection services) : base(services)
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
            return $"Serial type '{SerialType.Key}' updated";
        }

        private SerialType _serialType;

        private SerialType SerialType
        {
            get { return _serialType ??= _services.TranDb.GetSerialType(_commandData.SerialTypeId) 
                                         ?? throw new InvalidOperationException("Serial type with the provided ID does not exist."); }
        }
        
        public override void Preprocess()
        {
            SerialType.AuthorizationLevel = _commandData.AuthorizationLevel;
        }

        protected override void Execute()
        {
           _services.TranDb.UpdateSerialType(_commandInfo, SerialType);
        }
    }
}