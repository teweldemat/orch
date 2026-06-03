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
        public required string Dml { get; set; }
        public string? Note { get; set; }
    }

    public class ExecuteRawDmlCommandInitializer : CommandInitializerBase<ExecuteRawDmlCommand>
    {
        public ExecuteRawDmlCommandInitializer(IOHost host) : base(host)
        {
        }

        public override void Init()
        {
            if (string.IsNullOrEmpty(_commandData.Dml.Trim()))
            {
                throw new InvalidOperationException("DML cannot be null or empty.");
            }

            _commandData.Note = _commandData.Note?.Trim();
        }
    }

    public class ExecuteRawDmlCommandHandler : CommandHandlerBase<ExecuteRawDmlCommand>, ICommandHandler
    {
        public ExecuteRawDmlCommandHandler(TransactionServiceCollection services) : base(services)
        {
        }

        protected override void Authorize()
        {
            if (_commandInfo.UserId is not { } userId || _services.TranDb.GetRootUser()?.Id != userId)
            {
                throw new InvalidOperationException("Only root user can execute this command.");
            }
        }

        public override void Preprocess()
        {
            // var lowerDml = _commandData.Dml.ToLower();
            // if (lowerDml.Contains("begin") || lowerDml.Contains("commit") || lowerDml.Contains("rollback"))
            // {
            //     throw new InvalidOperationException(
            //         "DML should not contain transaction statements (BEGIN, COMMIT, ROLLBACK).");
            // }
        }

        public override string Summarize(out bool html)
        {
            html = false;
            
            if (!string.IsNullOrEmpty(_commandData.Note))
            {
                return $"Executed raw DML ({_commandData.Note}): {_commandData.Dml}";
            }
            
            return $"Executed raw DML: {_commandData.Dml}";
        }

        protected override void Execute()
        {
            OCommon.AssertRootUser(_services.TranDb, _commandInfo.UserId);
            _services.TranDb.ExecuteDml(_commandInfo, _commandData.Dml);
        }
    }
}