using orch.core.model;

namespace orch.core.command
{
    [CommandType(
        TYPE_ID,
        COMMAND_TYPE_KEY,
        "Change User Password",
        initializer: typeof(ChangePasswordCommandInitializer),
        handler: typeof(ChangeUserPasswordCommandHandler))]
    public class ChangePasswordCommand
    {
        public const string COMMAND_TYPE_KEY = "SYS_CHANGE_PASSWORD";
        public const string TYPE_ID = "04960c2b-0d74-4369-9496-7a6c2633e5eb";
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }

        [OGeneratedData] public string UserName { get; set; }
    }

    public class ChangePasswordCommandInitializer : CommandInitializerBase<ChangePasswordCommand>
    {
        public ChangePasswordCommandInitializer(IOHost host) : base(host)
        {
        }

        public override void Init()
        {
            if (string.IsNullOrEmpty(_commandData.OldPassword) || string.IsNullOrEmpty(_commandData.NewPassword))
            {
                throw new InvalidOperationException("Passwords should not be empty.");
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(_commandData.NewPassword, UserInfoProps.PASSWORD_PATTERN))
            {
                throw new ArgumentException("New password must be at least 8 characters long and contain no spaces.");
            }
        }
    }

    public class ChangeUserPasswordCommandHandler : CommandHandlerBase<ChangePasswordCommand>, ICommandHandler
    {
        public ChangeUserPasswordCommandHandler(TransactionServiceCollection services) : base(services)
        {
        }

        protected override void Authorize()
        {
        }

        public override void Preprocess()
        {
            if (_commandInfo.UserId is not { } userId ||
                _services.TranDb.GetUserInfo(_commandInfo.UserId.Value, includePassword: true) is not { } userInfo)
                throw new UnauthorizedAccessException("You are not authorized to change passwords");

            if (!userInfo.PasswordHash.SequenceEqual(OSystemService.HashPassword(_commandData.OldPassword)))
                throw new UnauthorizedAccessException("Old password does not match.");

            if (_commandData.OldPassword == _commandData.NewPassword)
                throw new ArgumentException("Old and new passwords are the same.");

            _commandData.UserName = userInfo.UserName;
        }

        public override string Summarize(out bool html)
        {
            html = false;
            return $"Password changed by {_commandData.UserName}.";
        }

        protected override void Execute()
        {
            _services.TranDb.ChangePassword(
                _commandInfo,
                _commandInfo.UserId.Value!,
                OSystemService.HashPassword(_commandData.NewPassword));
        }
    }
}