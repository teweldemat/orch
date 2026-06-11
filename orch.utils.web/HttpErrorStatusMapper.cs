using Microsoft.AspNetCore.Http;

namespace orch.utils.web
{
    public static class HttpErrorStatusMapper
    {
        public static int GetStatusCode(Exception ex)
        {
            if (ex is UnauthorizedAccessException)
                return StatusCodes.Status401Unauthorized;

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

            if (message.StartsWith("User ", StringComparison.OrdinalIgnoreCase) &&
                message.EndsWith(" is disabled", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
    }
}
