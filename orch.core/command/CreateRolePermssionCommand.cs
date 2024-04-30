using orch.core.model;

namespace orch.core.command
{
    [CommandType(
        TYPE_ID,
        COMMAND_TYPE_KEY,
        "Create Role Permission",
        initializer: typeof(CreateRolePermissionCommandInitializer),
        handler: typeof(CreateRolePermssionCommandHandler)
    )]
    public class CreateRolePermssionCommand
    {
        public const string COMMAND_TYPE_KEY = "SYS_CREATE_ROLE_PERMISSION";
        public const string TYPE_ID = "ca81a9e7e0344fe6b07bf186ac5b5d59";
        public RolePermissionDto RolePermissionDto { get; set; }
        public Role Role { get; set; }
        public List<Permission> Permissions { get; set; } = new List<Permission>();
    }

    public class CreateRolePermissionCommandInitializer : CommandInitializerBase<CreateRolePermssionCommand>
    {
        public CreateRolePermissionCommandInitializer(IOHost host) : base(host)
        {
        }

        public override void Init()
        {
            if (_commandData.RolePermissionDto == null)
                throw new InvalidOperationException("RolePermissionDto must not be null.");

            if (string.IsNullOrEmpty(_commandData.RolePermissionDto.RoleName))
                throw new InvalidOperationException("Role name must not be empty.");

            if (_commandData.RolePermissionDto.PermissionNames == null || !_commandData.RolePermissionDto.PermissionNames.Any())
                throw new InvalidOperationException("At least one permission must be provided.");
        }
    }

    public class CreateRolePermssionCommandHandler : CommandHandlerBase<CreateRolePermssionCommand>, ICommandHandler
    {
        public CreateRolePermssionCommandHandler(TransactionServiceCollection services) : base(services)
        {
        }
        
        protected override void Authorize()
        {
            if (_commandInfo.UserId is not {} userId 
                || !_services.TranDb.IsPermitted(userId, CoreModule.PERMISSION_MANAGE_ROLES))
            {
                throw new UnauthorizedAccessException("You are not authorized to manage roles");
            }
        }

        public override string Summarize(out bool html)
        {
            html = false;
            return $"Permission(s) {_commandData.RolePermissionDto.PermissionNames} created";
        }

        public override void Preprocess()
        {
            var role = _services.TranDb.GetRole(_commandData.RolePermissionDto.RoleName);
            if (role != null)
            {
                _commandData.Role = role;
                _commandData.Permissions = _commandData.RolePermissionDto.PermissionNames
                    .Select(perm => _services.TranDb.GetPermission(perm))
                    .ToList();
            }

            if (_commandData.Role == null)
            {
                throw new InvalidOperationException("The role is null. Please provide a valid role.");
            }

            if (!_commandData.Permissions.Any())
            {
                throw new InvalidOperationException("There are no permissions associated with the role. Please provide at least one permission.");
            }
        }

        protected override void Execute()
        {
            var permissionIds = _commandData.Permissions.Select(x => x.Id).ToList();
            _services.TranDb.SetRolePermssions(_commandInfo, _commandData.Role.Id, permissionIds);
        }
    }
}
