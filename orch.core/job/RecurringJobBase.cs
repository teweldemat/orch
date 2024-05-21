using System.Formats.Asn1;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.DependencyInjection;
using orch.core.command;
using orch.core.model;

namespace orch.core.job;

public abstract class RecurringJobBase
{
    protected TransactionServiceCollection Services { get; }
    protected IOHost Host => Services.Host;

    protected RecurringJobBase(TransactionServiceCollection services)
    {
        Services = services;
    }

    public abstract string JobId { get; }
    public abstract Task Run(PerformContext? context, CancellationToken cancellationToken);
    public abstract void AddOrUpdate();
    public abstract void Remove();

    private UserInfo? _systemUser;
    protected UserInfo? SystemUser => _systemUser ??= Services.TranDb.GetSystemUser();

    private TransactionSystemInformation? _systemInformation;

    protected TransactionSystemInformation? SystemInformation =>
        _systemInformation ??= Services.TranDb.GetCurrentSystemInformation();

    protected void ExecuteScopedCommandUntyped(Guid typeId, int formatVersion, object data, out Guid tranId)
    {
        if (SystemUser is not { } user)
            throw new InvalidOperationException("System user is not set.");

        if (SystemInformation?.SystemId is not { } systemId || systemId == Guid.Empty)
            throw new InvalidOperationException("System information is not set.");

        using var scope = Services.TranService.Services.CreateScope();
        var transactionService = scope.ServiceProvider.GetRequiredService<OTransactionService>();
        transactionService.ExecuteCommandUntyped(
            user.Id,
            systemId,
            typeId,
            formatVersion,
            data,
            out tranId);
    }

    protected void AddEventLog(
        EventLogProps.LogLevel level,
        string message,
        string reference = null,
        object data = null)
    {
        ExecuteScopedCommandUntyped(
            Guid.Parse(AddEventLogCommand.TYPE_ID),
            0,
            new AddEventLogCommand()
            {
                EventLog = new EventLog()
                {
                    Level = level,
                    Message = message,
                    Reference = reference,
                    Data = Newtonsoft.Json.JsonConvert.SerializeObject(data)
                }
            },
            out _);
    }
}