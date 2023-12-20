using orch.core.model;

namespace orch.core.command
{
    [CommandType(
       TYPE_ID,
       COMMAND_TYPE_KEY,
       "Create User Command",
       initializer: typeof(CreateUserCommandInitializer),
       handler: typeof(CreateUserCommandHandler))]
    public class CreateUserCommand
    {
        public const string COMMAND_TYPE_KEY = "SYS_CREATE_USER";
        public const string TYPE_ID = "8401c1cd-f6d0-4ee1-8d80-78c02cd1a064";
        public UserInfo User { get; set; }
        public string Password { get; set; }

        // RootRoleId and RootPermissionId used only during root user creation
        [OGeneratedData]
        public Guid? RootRoleId { get; set; } = null;
        [OGeneratedData]
        public Guid? RootPermissionId { get; set; } = null;
    }

    public class CreateUserCommandInitializer : CommandInitializerBase<CreateUserCommand>
    {
        public CreateUserCommandInitializer(IOHost host) : base(host)
        {
        }

        public override void Init()
        {
            _commandData.User.Id = Host.NextGuid();
            _commandData.RootPermissionId = Host.NextGuid();
            _commandData.RootRoleId = Host.NextGuid();
            _commandData.User.Time = _commandInfo.Time;
            _commandData.User.Enabled = true;

            var _passwordHash = OSystemService.HashPassword(_commandData.Password);
            _commandData.User.PasswordHash = _passwordHash;
        }
    }


    public class CreateUserCommandHandler : CommandHandlerBase<CreateUserCommand>, ICommandHandler
    {
        public CreateUserCommandHandler(TransactionServiceCollection services) : base(services)
        {
        }

        public override string Summarize(out bool html)
        {
            html = false;
            return $"User {_commandData.User.UserName} created";
        }

        protected override void Execute()
        {
            var root = _services.TranDb.GetRootUser();

            if (root == null)
            {
                if (_commandInfo.UserId != null)
                    throw new InvalidOperationException("User can't be specified now as the root user is not defined yet.");

                if (!_commandData.User.UserName.Equals(UserInfoProps.USER_NAME_ROOT))
                    throw new InvalidOperationException("The first user that is created should be the root user");

                var permId = _commandData.RootPermissionId.Value;
                _services.TranDb.CreatePermission(_commandInfo, new Permission { Id = permId, PermissionKey = Permission.ROOT_PERMISSION, PermissionName = "System root permission", TranId = _commandInfo.Id });

                var roleId = _commandData.RootRoleId.Value;
                _services.TranDb.CreateRole(_commandInfo, new Role { Id = roleId, RoleName = "System root role", Key = "system-role-key", Description = "System root role" });

                _services.TranDb.SetRolePermssions(_commandInfo, roleId, new List<Guid> { permId });

                _commandData.User.Roles = new List<Guid> { roleId };
            }


            var test = _services.TranDb.GetUserInfo(_commandData.User.UserName);
            if (test != null)
            {
                throw new InvalidOperationException($"{_commandData.User.UserName} is already used");
            }

            if (root != null && _commandData.User.Roles != null && _commandData.User.Roles.Count > 0)
            {
                AssignRolesCommandHandler.CheckPermssionToAssignRoles(_services.TranDb, _commandInfo, root, _commandData.User.Id, _commandData.User.Roles);
            }

            _services.TranDb.CreateUser(_commandInfo, _commandData.User);
        }
    }

}