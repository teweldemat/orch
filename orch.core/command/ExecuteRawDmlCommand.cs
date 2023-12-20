namespace orch.core.command
{
    [CommandType(
        TYPE_ID,
        COMMAND_TYPE_KEY,
        "Set System ID",
        initializer: typeof(ExecuteRawDmlCommandInitializer),
        handler: typeof(ExecuteRawDmlCommandHandler))]
    public class ExecuteRawDmlCommand
    {
        public const string COMMAND_TYPE_KEY = "SYS_EXEC_RAW_DML";
        public const string TYPE_ID = "6ce34702-61ad-44e6-bbad-5ca4734cde5a";
        public string Dml { get; set; }
    }

    public class ExecuteRawDmlCommandInitializer : CommandInitializerBase<ExecuteRawDmlCommand>
    {
        public ExecuteRawDmlCommandInitializer(IOHost host) : base(host)
        {
        }

        public override void Init()
        {
            if (string.IsNullOrEmpty(_commandData.Dml))
            {
                throw new InvalidOperationException("DML cannot be null or empty.");
            }
        }
    }

    public class ExecuteRawDmlCommandHandler : CommandHandlerBase<ExecuteRawDmlCommand>, ICommandHandler
    {
        public ExecuteRawDmlCommandHandler(TransactionServiceCollection services) : base(services)
        {
        }

        public override void Preprocess()
        {
        }

        public override string Summarize(out bool html)
        {
            html = false;
            return $"Executed raw DML: {_commandData.Dml}";
        }

        protected override void Execute()
        {
            OCommon.AssertRootUser(_services.TranDb, _commandInfo.UserId);
            _services.TranDb.ExecuteDml(_commandInfo, _commandData.Dml);
        }
    }
}
