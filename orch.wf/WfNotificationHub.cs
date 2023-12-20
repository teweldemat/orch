using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using orch.core.auth;
using orch.wf.commands;
using System.Security.Claims;

namespace orch.wf
{

    [Authorize(AuthenticationSchemes = AccessTokenAuthHandler.SchemaName)]
    public class WfNotificationHub : Hub
    {

        public static readonly string NotificationMethod = "onNotification";
        public static readonly string GetUndeliveredNotificationsCountMethod = "onUndeliveredNotificationsCount";

        private readonly WfServiceCollection _services;

        public WfNotificationHub(WfServiceCollection services)
        {
            _services = services;
        }

        public async Task Subscribe()
        {
            var userId = await Authorize();

            if (userId == Guid.Empty) return;

            await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
        }

        public async Task SetNotificationDelivered(Guid notificationId, Guid systemId, int formatVersion = 0)
        {
            var userId = await Authorize();

            if (userId == Guid.Empty) throw new UnauthorizedAccessException();

            if (_services.WfDb.GetNotificationTarget(userId, notificationId)?.DeliveredOn == null)
            {
                _services.TranService.ExecuteCommand(
                    userId,
                    systemId,
                    formatVersion, new SetNotificationDeliveredCommand()
                    {
                        NotificationId = notificationId,
                        UserId = userId
                    },
                    out _);
            }

            var count = _services.WfDb.GetUndeliveredNotificationsCount(userId);

            await Clients.User(userId.ToString()).SendAsync(GetUndeliveredNotificationsCountMethod, count);
        }

        public async Task<int> GetUndeliveredNotificationsCount()
        {
            var userId = await Authorize();

            if (userId == Guid.Empty) throw new UnauthorizedAccessException();

            var count = _services.WfDb.GetUndeliveredNotificationsCount(userId);

            await Clients.Caller.SendAsync(GetUndeliveredNotificationsCountMethod, count);

            return count;
        }

        private async Task<Guid> Authorize()
        {
            var userClaim = Context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userClaim) || !Guid.TryParse(userClaim, out Guid userId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated.");
                return Guid.Empty;
            }

            return userId;
        }

    }
}
