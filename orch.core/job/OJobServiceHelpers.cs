using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace orch.core.job
{
    public static class OJobServiceHelpers
    {
        public static void AddOrUpdateRecurringJobs(this IServiceCollection services, bool removeExisting = true)
        {
            using var serviceProvider = services.BuildServiceProvider();

            var tranDb = serviceProvider.GetRequiredService<ITransactionDatabase>();

            if (tranDb.GetCurrentSystemInformation() is null)
            {
                // if it is not bootstrapped yet, don't add recurring jobs
                return;
            }

            if (removeExisting)
            {
                _ = serviceProvider.GetRequiredService<IRecurringJobManager>(); // getting the manager triggers storage initialization
                var storage = JobStorage.Current;

                using var connection = storage.GetConnection();
                foreach (var recurringJob in StorageConnectionExtensions.GetRecurringJobs(connection))
                {
                    RecurringJob.RemoveIfExists(recurringJob.Id);
                }
            }

            serviceProvider.GetRequiredService<OJobService>().AddOrUpdateRecurringJobs();
        }

    }
}
