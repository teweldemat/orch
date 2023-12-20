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
        public string UserName { get; set; }
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
        private UserInfo user;

        public ChangeUserEnabledStatusCommandHandler(TransactionServiceCollection services) : base(services)
        {
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

            OCommon.AssertRootUser(_services.TranDb, _commandInfo.UserId);

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
