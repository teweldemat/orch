using Microsoft.Extensions.DependencyInjection;

namespace orch.core.job
{
    public static class OJobServiceHelpers
    {
        public static void AddOrUpdateRecurringJobs(this IServiceCollection services)
        {
            using var serviceProvider = services.BuildServiceProvider();

            var tranDb = serviceProvider.GetRequiredService<ITransactionDatabase>();

            if (tranDb.GetCurrentSystemInformation() is null)
            {
                // if it is not bootstrapped yet, don't add recurring jobs
                return;
            }

            serviceProvider.GetRequiredService<OJobService>().AddOrUpdateRecurringJobs();
        }

    }
}
