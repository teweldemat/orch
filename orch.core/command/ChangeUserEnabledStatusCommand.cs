using orch.core.model;

namespace orch.core.command
{
    [CommandType(
        TYPE_ID,
        COMMAND_TYPE_KEY,
        "Change User Enabled Status",
        initializer: typeof(ChangeUserEnabledStatusCommandInitializer),
        handler: typeof(ChangeUserEnabledStatusCommandHandler))]
    public class ChangeUserEnabledStatusCommand
    {
        public const string COMMAND_TYPE_KEY = "SYS_SET_ENABLED_STATUS";
        public const string TYPE_ID = "74cf2717-40e6-4cfd-a286-a2ae300ae09f";
        public Guid UserId { get; set; }
        public bool Enabled { get; set; }

        [OGeneratedData]
        public string UserName { get; set; } = string.Empty;
    }

    public class ChangeUserEnabledStatusCommandInitializer : CommandInitializerBase<ChangeUserEnabledStatusCommand>
    {
        public ChangeUserEnabledStatusCommandInitializer(IOHost host) : base(host)
        {
        }

        public override void Init()
        {
            if (_commandData.UserId == Guid.Empty)
                throw new InvalidDataException("The UserId property must not contain an empty identifier.");
        }
    }

    public class ChangeUserEnabledStatusCommandHandler : CommandHandlerBase<ChangeUserEnabledStatusCommand>, ICommandHandler
    {
        private UserInfo user = null!;

        public ChangeUserEnabledStatusCommandHandler(TransactionServiceCollection services) : base(services)
        {
        }
        
        protected override void Authorize()
        {
            var rootUser = _services.TranDb.GetRootUser()
                           ?? throw new InvalidOperationException(
                               "Root user not found, system has not been initialized.");
            
            var systemUser = _services.TranDb.GetSystemUser()
                    ?? throw new InvalidOperationException("System user not found, system has not been initialized.");

            if (_commandInfo.UserId is not { } userId)
                throw new UnauthorizedAccessException("You are not authorized to change user enabled status");

            var isRoot = _commandInfo.UserId == rootUser.Id;
            
            if (_commandData.UserId == rootUser.Id)
                throw new UnauthorizedAccessException("The root user's enabled status cannot be changed");
            
            if (_commandData.UserId == systemUser.Id)
                throw new UnauthorizedAccessException("The system user's enabled status cannot be changed");
            
            if (userId == _commandData.UserId)
                throw new InvalidOperationException("You cannot change your own enabled status");

            if (!isRoot && !_services.TranDb.IsPermitted(userId, CoreModule.PERMISSION_CREATE_USER))
                throw new UnauthorizedAccessException("You are not authorized to change user enabled status");
        }

        public override string Summarize(out bool html)
        {
            html = false;
            return $"User {user.UserName} {(user.Enabled ? "enabled" : "disabled")}";
        }

        public override void Preprocess()
        {
            user = _services.TranDb.GetUserInfo(_commandData.UserId);
            if (user == null)
                throw new InvalidDataException($"User with Id:{_commandData.UserId} doesn't exist");
            
            _commandData.UserName = user.UserName;

            if (user.Enabled && _commandData.Enabled)
                throw new InvalidDataException($"User with Id:{_commandData.UserId} already enabled");
            if (!user.Enabled && !_commandData.Enabled)
                throw new InvalidDataException($"User with Id:{_commandData.UserId} already disabled");
        }

        protected override void Execute()
        {
            _services.TranDb.ChangeUserEnabledStatus(_commandInfo, _commandData.UserId, _commandData.Enabled);
        }
    }
}
