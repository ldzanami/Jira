using Jira.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jira.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class TestGetterController(AppDbContext context) : ControllerBase
    {
        public AppDbContext Context { get; set; } = context;

        [HttpGet("get")]
        public IActionResult Get() => Ok();

        [HttpGet("getProjectMaster")]
        public async Task<IActionResult> GetMaster([FromQuery] string id)
        {
            return Ok((await Context.Projects.Where(proj => proj.ProjectMasterId == id).Include(u => u.ProjectMaster).ToListAsync()).FirstOrDefault()?.ProjectMaster);
        }
    }
}
