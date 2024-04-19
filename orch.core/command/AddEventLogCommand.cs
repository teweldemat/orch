using orch.core.logging;
using orch.core.model;

namespace orch.core.command;

[CommandType(
    TYPE_ID,
    COMMAND_TYPE_KEY,
    "Add Event Log Command",
    initializer: typeof(AddEventLogCommandInitializer),
    handler: typeof(AddEventLogCommandHandler))]
public class AddEventLogCommand
{
    public const string COMMAND_TYPE_KEY = "ADD_EVENT_LOG";
    public const string TYPE_ID = "1c2df8dc-ab82-438e-83d7-542c6265ac7f";
    public EventLog EventLog { get; set; }
}

public class AddEventLogCommandInitializer : CommandInitializerBase<AddEventLogCommand>
{
    public AddEventLogCommandInitializer(IOHost host) : base(host)
    {
    }

    public override void Init()
    {
        if (_commandData.EventLog is null)
            throw new InvalidDataException("The EventLog property must not be null.");

        if (_commandData.EventLog.Id == Guid.Empty)
            Host.NextGuid();

        _commandData.EventLog.Time = Host.CurrentTime();
    }
}

public class AddEventLogCommandHandler : CommandHandlerBase<AddEventLogCommand>, ICommandHandler
{
    private readonly IEventLogDatabase _eventLogDb;

    public AddEventLogCommandHandler(TransactionServiceCollection services, IEventLogDatabase eventLogDb) :
        base(services)
    {
        _eventLogDb = eventLogDb;
    }

    protected sealed override void Authorize()
    {
        if (_commandInfo.UserId is null || _services.TranDb.GetUserInfo(_commandInfo.UserId.Value) is not
                { UserName: { } userName })
        {
            throw new UnauthorizedAccessException("User information is required.");
        }

        if (userName != UserInfoProps.USER_NAME_ROOT && userName != UserInfoProps.USER_NAME_SYSTEM)
        {
            throw new UnauthorizedAccessException("You are not authorized to perform this operation.");
        }
    }

    public override string Summarize(out bool html)
    {
        html = false;
        return $"Add {_commandData.EventLog.Level} event log";
    }

    public override void Preprocess()
    {
    }

    protected override void Execute()
    {
        _eventLogDb.Add(_commandData.EventLog);
    }
}