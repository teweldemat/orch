using Microsoft.AspNetCore.Http;

namespace orch.utils.web
{
    public static class BrowserRequestHelper
    {
        public static bool PrefersHtml(HttpRequest request)
        {
            if (!request.Headers.TryGetValue("Accept", out var acceptHeader))
                return false;

            var accept = acceptHeader.ToString();
            if (string.IsNullOrWhiteSpace(accept))
                return false;

            return accept.Contains("text/html", StringComparison.OrdinalIgnoreCase)
                && !accept.Contains("application/json", StringComparison.OrdinalIgnoreCase);
        }
    }
}
