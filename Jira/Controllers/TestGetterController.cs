using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jira.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class TestGetterController : ControllerBase
    {
        [HttpGet("get")]
        public IActionResult Get() => Ok();
    }
}
