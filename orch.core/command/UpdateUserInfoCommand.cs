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
            if (_commandInfo.UserId is not {} userId)
                throw new UnauthorizedAccessException("You are not authorized to update user information.");
            
            var rootUser = _services.TranDb.GetRootUser()
                ?? throw new InvalidOperationException("Root user not found, system has not been initialized.");
            
            var targetIsRoot = _services.TranService.IsRootUser(_commandData.UserInfo.Id);
            if (targetIsRoot && rootUser.Id != userId)
                throw new UnauthorizedAccessException("You are not authorized to update root user information.");
             
            if (userId != rootUser.Id && !_services.TranDb.IsPermitted(_commandInfo.UserId.Value, CoreModule.PERMISSION_CREATE_USER))
                throw new UnauthorizedAccessException("You are not authorized to update user information.");
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
