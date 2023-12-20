using orch.core.model;

namespace orch.core.command
{
    [CommandType(
        TRAN_ID,
        COMMAND_TYPE_KEY,
        "Set Admin Permssion Information",
        initializer: typeof(SetPermssionAdminInfoCommandInitializer),
        handler: typeof(SetPermssionAdminInfoCommandHandler))]
    public class SetPermssionAdminInformationCommand
    {
        public const string COMMAND_TYPE_KEY = "SYS_SET_PERMISSION_ADMIN";
        public const string TRAN_ID = "90fccdb8-20ae-4767-9193-dc60d39b5caf";
        public PermissionAdminInfo PermissionAdminInfo { get; set; }

        // Remark: This is a placeholder GUID that may or may not be used later
        [OGeneratedData]
        public Guid NewPermissionId { get; set; }
    }

    public class SetPermssionAdminInfoCommandInitializer : CommandInitializerBase<SetPermssionAdminInformationCommand>
    {
        public SetPermssionAdminInfoCommandInitializer(IOHost host) : base(host)
        {
        }

        public override void Init()
        {
            // Assign a placeholder GUID that may or may not be used later
            _commandData.NewPermissionId = Host.NextGuid();
        }
    }

    public class SetPermssionAdminInfoCommandHandler : CommandHandlerBase<SetPermssionAdminInformationCommand>, ICommandHandler
    {
        bool isNew = false;

        public SetPermssionAdminInfoCommandHandler(TransactionServiceCollection services) : base(services)
        {
        }

        public override string Summarize(out bool html)
        {
            html = false;
            return $"Permission administrator information set: {_commandData.PermissionAdminInfo.PermissionAdminKey}";
        }

        public override void Preprocess()
        {
            var existing = _services.TranDb.GetPermissionAdminInfo(_commandData.PermissionAdminInfo.PermissionAdminKey);
            isNew = existing == null;

            if (isNew)
            {
                _commandData.PermissionAdminInfo.TranId = _commandInfo.Id;
                var isValid = PermissionProps.ValidatePermissionKey(_commandData.PermissionAdminInfo.PermissionAdminKey);
                if (!isValid)
                {
                    throw new InvalidOperationException("Invalid permssion admin key format");
                }
            }
        }

        protected override void Execute()
        {
            OCommon.AssertRootUser(_services.TranDb, _commandInfo.UserId);

            if (isNew)
            {
                _services.TranDb.CreatePermission(_commandInfo, new Permission { Id = _commandData.NewPermissionId, PermissionKey = _commandData.PermissionAdminInfo.PermissionAdminKey, PermissionName = _commandData.PermissionAdminInfo.PermssionAdminName, TranId = _commandInfo.Id });
                _services.TranDb.CreatePermissionAdminDefination(_commandInfo, _commandData.PermissionAdminInfo);
            }
            else
            {
                _services.TranDb.UpdatePermssionAdminDefination(_commandInfo, _commandData.PermissionAdminInfo);
            }
        }
    }
}
