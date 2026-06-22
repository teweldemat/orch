using Microsoft.AspNetCore.Http;
using System.Security.Authentication;

namespace orch.utils.web
{
    public static class HttpErrorStatusMapper
    {
        public static int GetStatusCode(Exception ex)
        {
            // Authentication failures (no/invalid/expired credentials) are 401.
            // AuthenticationException is purely about proving identity.
            if (ex is AuthenticationException)
                return StatusCodes.Status401Unauthorized;

            // Authorization failures (authenticated but not permitted, e.g. missing
            // permission or workflow team role) are 403, not 401. Returning 401 here
            // makes clients treat a permission denial as an expired session and bounce
            // the user back to the login flow instead of surfacing the real message.
            if (ex is UnauthorizedAccessException)
                return StatusCodes.Status403Forbidden;

            if (ex is FileNotFoundException or KeyNotFoundException)
                return StatusCodes.Status404NotFound;

            if (ex is InvalidOperationException invalidOperation &&
                IsAuthenticationFailure(invalidOperation.Message))
                return StatusCodes.Status401Unauthorized;

            return StatusCodes.Status500InternalServerError;
        }

        internal static bool IsAuthenticationFailure(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return false;

            if (message.Contains("Incorrect username and/or password", StringComparison.OrdinalIgnoreCase))
                return true;

            if (message.StartsWith("Too many failed login attempts", StringComparison.OrdinalIgnoreCase))
                return true;

            if (message.StartsWith("Too many login attempts", StringComparison.OrdinalIgnoreCase))
                return true;

            if (message.StartsWith("Access token not provided", StringComparison.OrdinalIgnoreCase))
                return true;

            if (message.Contains("Access Token", StringComparison.OrdinalIgnoreCase) &&
                message.Contains("is not valid", StringComparison.OrdinalIgnoreCase))
                return true;

            if (message.Contains("Access token", StringComparison.OrdinalIgnoreCase) &&
                message.Contains("has expired", StringComparison.OrdinalIgnoreCase))
                return true;

            if (message.Contains("Access token", StringComparison.OrdinalIgnoreCase) &&
                message.Contains("doesn't exist", StringComparison.OrdinalIgnoreCase))
                return true;

            if (message.StartsWith("User ", StringComparison.OrdinalIgnoreCase) &&
                message.EndsWith(" is disabled", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
    }
}
