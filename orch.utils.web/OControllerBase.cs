using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace orch.utils.web
{
    public class OControllerBase : Controller
    {
        // TODO: Move to a global exception handler (middleware) for centralized error management.
        protected IActionResult Error(Exception ex)
        {
            return Error(ex, ex.Message);
        }

        protected IActionResult Error(Exception ex, string message)
        {
            var statusCode = HttpErrorStatusMapper.GetStatusCode(ex);
            var htmlResult = TryUnauthorizedHtmlResult(statusCode, message);
            if (htmlResult != null)
                return htmlResult;

            htmlResult = TryBrowserHtmlErrorResult(statusCode, message);
            if (htmlResult != null)
                return htmlResult;

            return StatusCode(statusCode, CreateErrorInfo(message, ex));
        }

        protected virtual IActionResult TryBrowserHtmlErrorResult(int statusCode, string message)
        {
            return null!;
        }

        protected virtual IActionResult TryUnauthorizedHtmlResult(int statusCode, string message)
        {
            return null!;
        }

        protected ErrorInfo CreateErrorInfo(string message, Exception ex)
        {
            var includeStackTrace = HttpContext.RequestServices?
                .GetService<IWebHostEnvironment>()?.IsDevelopment() == true;
            return new ErrorInfo(message, ex, includeStackTrace);
        }
    }
}