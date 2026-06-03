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
        [OGeneratedData] public string UserName { get; set; } = string.Empty;
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
            if (command.UserId is not { } commandUserId)
                throw new UnauthorizedAccessException("You are not authorized to assign roles");

            var isRoot = commandUserId == rootUser.Id;
            if (!isRoot)
            {
                if (!db.IsPermitted(commandUserId, CoreModule.PERMISSION_ASSIGN_ROLE))
                    throw new UnauthorizedAccessException("You are not authorized to assign roles");
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
            var root = _services.TranDb.GetRootUser()
                ?? throw new InvalidOperationException("Root user not found, system has not been initialized.");
            CheckPermssionToAssignRoles(_services.TranDb, _commandInfo, root, _commandData.UserId, _commandData.Roles);
            _services.TranDb.SetUserRole(_commandInfo, _commandData.UserId, _commandData.Roles);
        }
    }

}