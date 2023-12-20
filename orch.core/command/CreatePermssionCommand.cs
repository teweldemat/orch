using orch.core.model;
using System.Text.RegularExpressions;

namespace orch.core.command
{
    [CommandType(
        TYPE_ID,
        COMMAND_TYPE_KEY,
        "Create Permission",
        initializer: typeof(CreatePermissionCommandInitializer),
        handler: typeof(CreatePermissionCommandHandler)
    )]
    public class CreatePermssionCommand
    {
        public const string COMMAND_TYPE_KEY = "SYS_CREATE_PERMISSION";
        public const string TYPE_ID = "557A76D2-3BD5-4CE1-80E8-895314EA2B93";
        public List<Permission> Permissions { get; set; } = new List<Permission>();
    }

    public class CreatePermissionCommandInitializer : CommandInitializerBase<CreatePermssionCommand>
    {
        public CreatePermissionCommandInitializer(IOHost host) : base(host)
        {
        }


        public override void Init()
        {
            if (_commandData.Permissions == null || _commandData.Permissions.Count == 0)
                throw new InvalidOperationException("No permissions provided.");

            foreach (var perm in _commandData.Permissions)
            {
                var m = Regex.Match(perm.PermissionKey, Permission.PERMISSOIN_KEY_PATTERN);
                if (!m.Success || m.Length != perm.PermissionKey.Length)
                    throw new InvalidOperationException($"Invalid permission key {perm.PermissionKey}");

                if (string.IsNullOrEmpty(perm.PermissionName))
                    throw new InvalidOperationException("Permission name not provided");

                perm.Id = Host.NextGuid();
                perm.TranId = _commandInfo.Id;
            }
        }
    }

    public class CreatePermissionCommandHandler : CommandHandlerBase<CreatePermssionCommand>, ICommandHandler
    {
        public CreatePermissionCommandHandler(TransactionServiceCollection services) : base(services)
        {
        }

        public override string Summarize(out bool html)
        {
            html = false;
            var permissions = string.Join(", ", _commandData.Permissions.ConvertAll(p => p.PermissionKey));
            return $"Permission(s) {permissions} created";
        }

        public override void Preprocess()
        {
        }

        protected override void Execute()
        {
            OCommon.AssertRootUser(_services.TranDb, _commandInfo.UserId);

            foreach (var perm in _commandData.Permissions)
            {
                var test = _services.TranDb.GetPermission(perm.PermissionKey);
                if (test != null)
                    throw new InvalidOperationException($"Permission key {perm.PermissionKey} already exists");

                _services.TranDb.CreatePermission(_commandInfo, perm);
            }
        }
    }
}
