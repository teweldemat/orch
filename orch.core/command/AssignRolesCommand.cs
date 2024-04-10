using orch.core.model;

namespace orch.core.command
{
    [CommandType(
        TYPE_ID,
        COMMAND_TYPE_KEY,
        "Assign Roles Command",
        initializer: typeof(AssignRolesCommandInitializer),
        handler: typeof(AssignRolesCommandHandler))]
    public class AssignRolesCommand
    {
        public const string COMMAND_TYPE_KEY = "SYS_ASSIGN_ROLE";
        public const string TYPE_ID = "c4e948bc-884a-424d-b2a9-62a361d7465f";
        public Guid UserId { get; set; }
        public List<Guid> Roles { get; set; } = new List<Guid>();
        [OGeneratedData] public string UserName { get; set; }
    }    

    public class AssignRolesCommandInitializer : CommandInitializerBase<AssignRolesCommand>
    {
        public AssignRolesCommandInitializer(IOHost host) : base(host)
        {
        }

        public override void Init()
        {
            if (_commandData.UserId == Guid.Empty)
                throw new InvalidDataException("The UserId property must not contain an empty identifier.");

            if (!_commandData.Roles.Any())
                throw new InvalidDataException("The Roles property must contain at least one role identifier.");
        }
    }
    public class AssignRolesCommandHandler : CommandHandlerBase<AssignRolesCommand>, ICommandHandler
    {
        public AssignRolesCommandHandler(TransactionServiceCollection services) : base(services)
        {
        }

        public override void Preprocess()
        {
            _commandData.UserName = _services.TranDb.GetUserInfo(_commandData.UserId).UserName;
        }

        public static void CheckPermssionToAssignRoles(ITransactionDatabase db, OCommand command, UserInfo rootUser, Guid userId, List<Guid> roles)
        {
            var isRoot = command.UserId.Value == rootUser.Id;
            if (!isRoot) //if user not root he/she needs PERMSSION_ADMINISTRATOR permssion
            {
                {
                    // 'PermissionAdmin' feature is incomplete
                    // Let's use a simple permission check for now

                    if (!db.IsPermitted(command.UserId.Value, CoreModule.PERMISSION_ASSIGN_ROLE))
                        throw new UnauthorizedAccessException("You are not authorized to assign roles");

                    return;
                }

                var userPermssions = db.GetUserPermissions(command.UserId.Value);
 
                var permAdminKeys = new List<string>();
                foreach (var perm in userPermssions)
                {
                    var permKey = db.GetPermission(perm).PermissionKey;
                    if (permKey.StartsWith(PermissionProps.PERMISSION_ADMIN_PREFIX))
                    {
                        permAdminKeys.Add(permKey);
                    }
                }
                var adminableKeys = new HashSet<Guid>();
                foreach (var pk in permAdminKeys)
                {
                    var permAdmin = db.GetPermissionAdminInfo(pk);
                    foreach (var key in permAdmin.PermissionKeys)
                    {
                        if (key.Contains(PermissionProps.WILD_CARD_CHAR))
                        {
                            foreach (var k in db.ExpandPermssions(key))
                                adminableKeys.Add(k.Id);
                        }
                        else
                            adminableKeys.Add(db.GetPermission(key).Id);
                    }
                }
                foreach (var role in roles)
                {
                    var r = db.GetRole(role);
                    foreach (var rp in r.Permissions)
                    {
                        if (!adminableKeys.Contains(rp))
                            throw new InvalidOperationException("User not authorized to give these permssions");
                    }
                }
            }
            foreach (var role in roles)
            {
                var testrole = db.GetRole(role);
                if (testrole == null)
                    throw new InvalidOperationException($"Invalid role id: {role}");
            }
        }

        public override string Summarize(out bool html)
        {
            html = false;
            return $"Assigned {_commandData.Roles.Count} roles";
        }

        protected override void Execute()
        {
            var root = _services.TranDb.GetRootUser();
            CheckPermssionToAssignRoles(_services.TranDb, _commandInfo, root, _commandData.UserId, _commandData.Roles);
            _services.TranDb.SetUserRole(_commandInfo, _commandData.UserId, _commandData.Roles);
        }
    }

}