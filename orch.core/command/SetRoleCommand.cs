using orch.core.model;

namespace orch.core.command
{
    [CommandType(
        TYPE_ID,
        COMMAND_TYPE_KEY,
        "Set Role",
        initializer: typeof(SetRoleCommandInitializer),
        handler: typeof(SetRoleCommandHandler))]
    public class SetRoleCommand
    {
        public const string COMMAND_TYPE_KEY = "SYS_SET_ROLE";
        public const string TYPE_ID = "7a596d93-5ddd-407e-b211-8927fb2c78a0";
        public Role Role { get; set; }

        // Remark: This is a placeholder GUID that may or may not be used later
        [OGeneratedData]
        public Guid NewRoleId { get; set; }
    }

    public class SetRoleCommandInitializer : CommandInitializerBase<SetRoleCommand>
    {
        public SetRoleCommandInitializer(IOHost host) : base(host)
        {
        }

        public override void Init()
        {
            _commandData.NewRoleId = Host.NextGuid();
        }
    }

    public class SetRoleCommandHandler : CommandHandlerBase<SetRoleCommand>, ICommandHandler
    {
        bool _newRole = false;

        public SetRoleCommandHandler(TransactionServiceCollection services) : base(services)
        {
        }

        public override string Summarize(out bool html)
        {
            html = false;
            return $"Role {_commandData.Role.RoleName} created";
        }

        public override void Preprocess()
        {
            if (_commandData.Role.Id == Guid.Empty)
            {
                _commandData.Role.Id = _commandData.NewRoleId;
                _newRole = true;
            }
            else if (_commandData.NewRoleId == _commandData.Role.Id)
            {
                _newRole = true;
            }


            _commandData.Role.RoleName = _commandData.Role.RoleName.Trim();

            if (!RoleProps.ValidateRoleName(_commandData.Role.RoleName))
            {
                throw new FormatException("Role name not allowed");
            }

            foreach (var perm in _commandData.Role.Permissions)
            {
                var test = _services.TranDb.GetPermission(perm);
                if (test == null)
                {
                    throw new InvalidOperationException($"Permssion id {perm} doesn't exists");
                }
            }
        }

        protected override void Execute()
        {
            OCommon.AssertRootUser(_services.TranDb, _commandInfo.UserId);

            if (_newRole)
            {
                _services.TranDb.CreateRole(_commandInfo, _commandData.Role);
            }
            else
            {
                _services.TranDb.UpdateRole(_commandInfo, _commandData.Role);
            }
        }
    }
}
