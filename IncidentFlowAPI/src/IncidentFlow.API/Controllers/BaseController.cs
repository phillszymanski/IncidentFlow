using Microsoft.AspNetCore.Mvc;

namespace IncidentFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseController : ControllerBase
    {
        protected IActionResult HandleResult<T>(T result)
        {
            if (result == null)
                return NotFound();
            return Ok(result);
        }
    }
}
