using Microsoft.AspNetCore.Mvc;

namespace orch.utils.web
{
    public class OControllerBase : Controller
    {
        // TODO: Move to a global exception handler (middleware) for centralized error management.
        protected IActionResult Error(Exception ex)
        {
            return StatusCode(HttpErrorStatusMapper.GetStatusCode(ex), new ErrorInfo(ex.Message, ex));
        }

        protected IActionResult Error(Exception ex, string message)
        {
            return StatusCode(HttpErrorStatusMapper.GetStatusCode(ex), new ErrorInfo(message, ex));
        }
    }
}