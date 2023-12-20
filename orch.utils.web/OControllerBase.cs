using Microsoft.AspNetCore.Mvc;

namespace orch.utils.web
{
    public class OControllerBase : Controller
    {
        protected IActionResult Error(Exception ex)
        {
            return StatusCode(500, new ErrorInfo(null, ex));
        }
        protected IActionResult Error(Exception ex, string message)
        {
            return StatusCode(500, new ErrorInfo(message, ex));
        }
    }
}