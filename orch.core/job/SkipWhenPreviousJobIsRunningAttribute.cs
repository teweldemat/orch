using Hangfire;
using Hangfire.Client;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace orch.core.job;

public class SkipWhenPreviousJobIsRunningAttribute: JobFilterAttribute, IClientFilter, IApplyStateFilter
{
    public void OnCreating(CreatingContext context)
    {
        // We can't handle old storages
        if (context.Connection is not JobStorageConnection connection) return;

        // We should run this filter only for background jobs based on 
        // recurring ones
        if (!context.Parameters.ContainsKey("RecurringJobId")) return;

        var recurringJobId = context.Parameters["RecurringJobId"] as string;

        // RecurringJobId is malformed. This should not happen, but anyway.
        if (string.IsNullOrWhiteSpace(recurringJobId)) return;

        var running = connection.GetValueFromHash($"recurring-job:{recurringJobId}", "Running");
        if ("yes".Equals(running, StringComparison.OrdinalIgnoreCase))
        {
            context.Canceled = true;
        }
    }

    public void OnCreated(CreatedContext filterContext)
    {
    }

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        var recurringJobId = SerializationHelper.Deserialize<string>(context.Connection.GetJobParameter(context.BackgroundJob.Id, "RecurringJobId"));
        if (string.IsNullOrWhiteSpace(recurringJobId)) return;
        
        if (context.NewState is EnqueuedState)
        {
            transaction.SetRangeInHash(
                $"recurring-job:{recurringJobId}",
                new[] { new KeyValuePair<string, string>("Running", "yes") });
        }
        else if (context.NewState.IsFinal /* || context.NewState is FailedState*/)
        {
            var job = JobStorage.Current.GetConnection().GetRecurringJobs(new[] { recurringJobId }).FirstOrDefault();
            if(job is { Removed: true})  return;
            
            transaction.SetRangeInHash(
                $"recurring-job:{recurringJobId}",
                new[] { new KeyValuePair<string, string>("Running", "no") });
        }
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
    }
}