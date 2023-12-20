using orch.core.model;

namespace orch.core.command
{
    [CommandType(
        TYPE_ID,
        COMMAND_TYPE_KEY,
        "Delete User Command",
        initializer: typeof(DeleteUserCommandInitializer),
        handler: typeof(DeleteUserCommandHandler))]
    public class DeleteUserCommand
    {
        public const string COMMAND_TYPE_KEY = "SYS_DELETE_USER";
        public const string TYPE_ID = "9daf010b-691c-4e31-a88b-42cbfb22c9bf";
        public Guid UserId { get; set; }
        public UserInfo DeletedUser { get; set; }
    }

    public class DeleteUserCommandInitializer : CommandInitializerBase<DeleteUserCommand>
    {
        public DeleteUserCommandInitializer(IOHost host) : base(host)
        {
        }

        public override void Init()
        {
            if (_commandData.UserId == Guid.Empty)
                throw new InvalidDataException("UserId cannot be an empty Guid.");
        }
    }

    public class DeleteUserCommandHandler : CommandHandlerBase<DeleteUserCommand>, ICommandHandler
    {
        public DeleteUserCommandHandler(TransactionServiceCollection services) : base(services)
        {
        }

        public override void Preprocess()
        {
            _commandData.DeletedUser = _services.TranDb.GetUserInfo(_commandData.UserId);

            if (_commandData.DeletedUser == null)
                throw new InvalidOperationException($"User with ID {_commandData.UserId} does not exist.");

            var root = _services.TranDb.GetRootUser();

            if (_commandData.DeletedUser.Id.Equals(root.Id))
                throw new InvalidOperationException("Root user cannot be deleted.");
        }

        public override string Summarize(out bool html)
        {
            html = false;
            return $"User {_commandData.DeletedUser.UserName} deleted";
        }

        protected override void Execute()
        {
            OCommon.AssertRootUser(_services.TranDb, _commandInfo.UserId);
            _services.TranDb.DeleteUser(_commandInfo, _commandData.UserId);
        }
    }
}
