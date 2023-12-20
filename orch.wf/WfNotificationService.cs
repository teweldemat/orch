using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace orch.wf
{
    public class WfNotificationService
    {
        private readonly IHubContext<WfNotificationHub> _hubContext;
        private readonly IServiceScopeFactory _scopeFactory;

        public WfNotificationService(
            IHubContext<WfNotificationHub> hubContext,
            IServiceScopeFactory scopeFactory)
        {
            _hubContext = hubContext;
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        /// Enqueues a notification to be sent asynchronously to the specified targets.
        /// </summary>
        /// <param name="Ids">The Ids of the notifications to be sent.</param>
        /// <returns>The Id of the background job.</returns>
        public string SendNotifications(params Guid[] Ids)
        {
            var jobId = BackgroundJob.Enqueue(() => EnqueueNotificationsJob(Ids));
            return jobId;
        }

        /// <summary>
        /// Background job to send multiple notifications. A failure in sending one notification
        /// does not prevent the others from being sent.
        /// </summary>
        /// <param name="Ids">An array of notification Ids to send.</param>
        public async Task EnqueueNotificationsJob(Guid[] Ids)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IWfDatabase>();

            var tasks = new List<Task>();

            foreach (var id in Ids)
            {
                var notification = db.GetNotification(id);
                if (notification == null)
                {
                    continue; // Skip this notification and proceed with the next one.
                }

                var targets = db.GetNotificationTargets(id);
                if (targets == null || !targets.Any())
                {
                    continue; // Skip this notification and proceed with the next one.
                }

                JsonSerializerSettings settings = new()
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = null
                    }
                };

                var serializedNotification = JsonConvert.SerializeObject(notification, settings);

                foreach (var target in targets)
                {
                    tasks.Add(_hubContext.Clients.User(target.UserId.ToString()).SendAsync(
                        WfNotificationHub.NotificationMethod, serializedNotification)
                        .ContinueWith(task =>
                        {
                            if (task.IsFaulted)
                            {
                                // Do nothing
                            }
                        }));
                }
            }

            await Task.WhenAll(tasks);
        }
    }
}
