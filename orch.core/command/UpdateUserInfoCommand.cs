using orch.core.model;
using System.Data;

namespace orch.core.command
{
    [CommandType(
        TYPE_ID,
        COMMAND_TYPE_KEY,
        "Update user information",
        initializer: typeof(UpdateUserInfoCommandInitializer),
        handler: typeof(UpdateUserInfoCommandHandler)
    )]
    public class UpdateUserInfoCommand
    {
        public const string COMMAND_TYPE_KEY = "SYS_UPDATE_USER";
        public const string TYPE_ID = "a1d9bef1-0d8c-4573-811b-f61e9e0aa153";

        public UserInfo UserInfo { get; set; }
    }

    public class UpdateUserInfoCommandInitializer : CommandInitializerBase<UpdateUserInfoCommand>
    {
        public UpdateUserInfoCommandInitializer(IOHost host) : base(host)
        {
        }

        public override void Init()
        {
            if (_commandData.UserInfo == null)
                throw new InvalidOperationException("User information not provided");

            if (_commandData.UserInfo.Id == Guid.Empty)
                throw new InvalidOperationException("User Id should not be empty.");
        }
    }

    public class UpdateUserInfoCommandHandler : CommandHandlerBase<UpdateUserInfoCommand>, ICommandHandler
    {
        UserInfo existing;
        public UpdateUserInfoCommandHandler(TransactionServiceCollection services) : base(services)
        {
        }

        protected override void Authorize()
        {
            var isRootUserBeingUpdated = _services.TranService.IsRootUser(_commandData.UserInfo.Id);
            var isRootUserRequesting = _services.TranService.IsRootUser((Guid)_commandInfo.UserId);

            if (isRootUserBeingUpdated && !isRootUserRequesting)
            {
                throw new UnauthorizedAccessException("Only a root user can update another root user.");
            }

            OCommon.AssertRootUser(_services.TranDb, this._commandInfo.UserId, false);
        }

        public override void Preprocess()
        {
            existing = _services.TranDb.GetUserInfo(_commandData.UserInfo.Id);

            if (existing == null)
                throw new InvalidOperationException($"User with Id '{_commandData.UserInfo.Id}' does not exist.");


            if (existing.UserName != _commandData.UserInfo.UserName)
            {
                throw new InvalidOperationException("Updating the username is not permitted.");
            }
        }

        public override string Summarize(out bool html)
        {
            html = false;
            return $"User information updated for '{_commandData.UserInfo.UserName}'.";
        }

        protected override void Execute()
        {
            existing.FullName = _commandData.UserInfo.FullName;
            existing.PhoneNo = _commandData.UserInfo.PhoneNo;
            existing.Email = _commandData.UserInfo.Email;

            existing.EmployeeId = _commandData.UserInfo.EmployeeId;
            existing.ReaderId = _commandData.UserInfo.ReaderId;

            _services.TranDb.UpdateUserInfo(existing);
        }
    }
}
