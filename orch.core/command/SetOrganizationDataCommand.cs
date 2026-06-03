using orch.core.model;

namespace orch.core.command
{
    [CommandType(TYPE_ID, COMMAND_TYPE_KEY, "Set Organization Data",
        initializer = typeof(SetOrganizationDataCommandInitializer),
        handler = typeof(SetOrganizationDataCommandHandler))]
    public class SetOrganizationDataCommand
    {
        public const string COMMAND_TYPE_KEY = "SET_ORG_DATA";
        public const string TYPE_ID = "183a8fab-65ff-4dbe-98cd-6c43c0f17d03";
        public required OrganizationData OrgData { get; set; }
    }

    public class SetOrganizationDataCommandInitializer : CommandInitializerBase<SetOrganizationDataCommand>
    {
        public SetOrganizationDataCommandInitializer(IOHost host) : base(host)
        {
        }

        public override void Init()
        {
            if (_commandData.OrgData == null)
                throw new InvalidDataException("A valid Expense advance configuration should be provided.");
            _commandData.OrgData.Id = Host.NextGuid();

        }
    }
    public class SetOrganizationDataCommandHandler : CommandHandlerBase<SetOrganizationDataCommand>, ICommandHandler
    {
        public SetOrganizationDataCommandHandler(TransactionServiceCollection services) : base(services)
        {
        }

        public override string Summarize(out bool html)
        {
            html = false;
            return $"Organization data set";
        }
        protected override void Authorize()
        {
            if (_services.TranDb.GetRootUser() is not { } rootUser
                || _commandInfo.UserId is not { } userId
                || rootUser.Id == userId)
                throw new UnauthorizedAccessException("You are not allowed to change organization data");
        }
        public override void Preprocess()
        {
        }
        protected override void Execute()
        {
            _services.TranDb.SetOrganzationData(_commandInfo, _commandData.OrgData);
        }
    }
}
