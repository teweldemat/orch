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
            {
                throw new InvalidOperationException("UserId should not be empty.");
            }

            _commandData.Password = _commandData.Password.Trim();

            if (string.IsNullOrEmpty(_commandData.Password))
            {
                throw new InvalidOperationException("Password should not be empty.");
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
            var rootUser = _services.TranDb.GetRootUser()
                           ?? throw new InvalidOperationException(
                               "Root user not found, system has not been initialized.");

            if (_commandInfo.UserId is not { } userId)
                throw new UnauthorizedAccessException("You are not authorized to change passwords");

            var isRoot = _commandInfo.UserId == rootUser.Id;

            if (_commandData.UserId == rootUser.Id && !isRoot)
                throw new UnauthorizedAccessException("You are not authorized to change the root user's password");

            if (!isRoot && _commandInfo.UserId != _commandData.UserId &&
                !_services.TranDb.IsPermitted(userId, CoreModule.PERMISSION_CREATE_USER))
                throw new UnauthorizedAccessException("You are not authorized to change passwords");
        }


        public override void Preprocess()
        {
            _commandData.UserName = _services.TranDb.GetUserInfo(_commandData.UserId).UserName;
        }

        public override string Summarize(out bool html)
        {
            html = false;
            return $"Password changed for {_commandData.UserName}.";
        }

        protected override void Execute()
        {
            var passwordHash = OSystemService.HashPassword(_commandData.Password);
            _services.TranDb.ChangePassword(_commandInfo, _commandData.UserId, passwordHash);
        }
    }
}
