using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using orch.core.auth;
using System.Security.Claims;

namespace orch.core.job
{
    public class JobProgress
    {
        public int Current { get; set; }
        public int Total { get; set; }
    }

    [Authorize(AuthenticationSchemes = AccessTokenAuthHandler.SchemaName)]
    public class JobProgressHub : Hub
    {
        public static readonly string ProgressMethod = "onProgress";
        public static readonly string SuccessMethod = "onSuccess";
        public static readonly string CancelledMethod = "OnCancelled";

        private readonly OJobService _jobService;
        public JobProgressHub(OJobService jobService)
        {
            _jobService = jobService;
        }

        public async Task Subscribe(string jobId)
        {
            var userId = await Authorize();

            if (userId == Guid.Empty) throw new UnauthorizedAccessException();

            await Groups.AddToGroupAsync(Context.ConnectionId, jobId);
        }

        public async Task Cancel(string jobId)
        {
            var userId = await Authorize();

            if (userId == Guid.Empty) throw new UnauthorizedAccessException();

            _jobService.CancelJob(userId, jobId);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, jobId);
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
