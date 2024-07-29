using orch.core.model;

namespace orch.core.command
{
    [CommandType(
        TYPE_ID,
        COMMAND_TYPE_KEY,
        "Set User Password",
        initializer: typeof(SetPasswordCommandInitializer),
        handler: typeof(SetUserPasswordCommandHandler))]
    public class SetPasswordCommand
    {
        public const string COMMAND_TYPE_KEY = "SYS_SET_PASSWORD";
        public const string TYPE_ID = "cbacc7d6-1ff4-483c-96d9-c4d3162c4c2f";
        public Guid UserId { get; set; }
        public string Password { get; set; }

        [OGeneratedData] public string UserName { get; set; }
    }

    public class SetPasswordCommandInitializer : CommandInitializerBase<SetPasswordCommand>
    {
        public SetPasswordCommandInitializer(IOHost host) : base(host)
        {
        }

        public override void Init()
        {
            if (_commandData.UserId == Guid.Empty)
                throw new InvalidOperationException("Please specify the Id of the user to change the password for.");
            
            if (string.IsNullOrEmpty(_commandData.Password))
                throw new InvalidOperationException("Please specify a new password.");
            
            if (!System.Text.RegularExpressions.Regex.IsMatch(_commandData.Password, UserInfoProps.PASSWORD_PATTERN))
            {
                throw new ArgumentException("New password must be at least 8 characters long and contain no spaces.");
            }
        }
    }

    public class SetUserPasswordCommandHandler : CommandHandlerBase<SetPasswordCommand>, ICommandHandler
    {
        public SetUserPasswordCommandHandler(TransactionServiceCollection services) : base(services)
        {
        }

        protected override void Authorize()
        {
            if (_commandInfo.UserId is not { } userId)
                throw new UnauthorizedAccessException("You are not authorized to change passwords");
            
            var rootUser = _services.TranDb.GetRootUser();
            var systemUser = _services.TranDb.GetSystemUser();
                
            if (rootUser == null || systemUser == null)
                throw new InvalidOperationException("Root and/or system user not found, has the system been bootstrapped?");
            
            var targetIsRoot = _commandData.UserId == rootUser.Id;
            var targetIsSystem = _commandData.UserId == systemUser.Id;
            
            var actorIsRoot = _commandInfo.UserId == rootUser.Id;
            var actorIsSystem = _commandInfo.UserId == systemUser.Id;
            
            if (targetIsRoot)
            {
                if (!actorIsRoot)
                    throw new UnauthorizedAccessException("You are not authorized to change the root user's password");
            }
            else if (targetIsSystem)
            {
                if (!actorIsSystem && !actorIsRoot)
                    throw new UnauthorizedAccessException("You are not authorized to change the system user's password");
            }
            else if (!actorIsRoot && !actorIsSystem)
            {
                if (!_services.TranDb.IsPermitted(userId, CoreModule.PERMISSION_CHANGE_PASSWORD))
                    throw new UnauthorizedAccessException("You are not authorized to change passwords. Missing permission: " + CoreModule.PERMISSION_CHANGE_PASSWORD);
            }
        }
        
        private string _newPassword;
        public override void Preprocess()
        {
            if (_services.TranDb.GetUserInfo(_commandData.UserId) is not { } targetUser)
                throw new InvalidOperationException($"User with Id '{_commandData.UserId}' not found.");
            
            _commandData.UserName = targetUser.UserName;
            
            _newPassword = _commandData.Password;
            _commandData.Password = default;
        }

        public override string Summarize(out bool html)
        {
            html = false;
            return $"Password changed for {_commandData.UserName}.";
        }

        protected override void Execute()
        {
            var passwordHash = OSystemService.HashPassword(_newPassword);
            _services.TranDb.ChangePassword(_commandInfo, _commandData.UserId, passwordHash);
            
            _commandData.Password = string.Empty;
        }
    }
}
