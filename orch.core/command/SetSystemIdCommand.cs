namespace orch.core.command
{
    [CommandType(
        TYPE_ID,
        COMMAND_TYPE_KEY,
        "Set System ID",
        initializer: typeof(SetSystemIdCommandInitializer)
    )]
    public class SetSystemIdCommand
    {
        public const string COMMAND_TYPE_KEY = "SYS_SET_SYSTEM_ID";
        public const string TYPE_ID = "d843bd77-5092-4252-a62f-ff0254a3724c";
        public Guid SystemId { get; set; }
    }

    public class SetSystemIdCommandInitializer : CommandInitializerBase<SetSystemIdCommand>
    {
        public SetSystemIdCommandInitializer(IOHost host) : base(host)
        {
        }

        public override void Init()
        {
            if (_commandData.SystemId == Guid.Empty)
                throw new InvalidOperationException("SystemId must not be empty.");
        }
    }
}
