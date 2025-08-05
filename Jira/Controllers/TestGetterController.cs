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
        private AppDbContext Context { get; init; } = context;

        [HttpGet("get")]
        public IActionResult Get() => Ok();

        [HttpGet("getProjectMaster")]
        public async Task<IActionResult> GetMaster([FromQuery] string id)
        {
            return Ok((await Context.Projects.Where(proj => proj.Id == id).Include(u => u.Owner).ToListAsync()).FirstOrDefault()?.Owner);
        }

        [HttpGet("{id}/members")]
        public async Task<IActionResult> GetMembers([FromRoute] string id) => Ok(await Context.Projects.Include(proj => proj.ProjectMembers)
                                                                                                       .ThenInclude(memer => memer.User)
                                                                                                       .Select(proj => new {
                                                                                                           proj.Id,
                                                                                                           proj.Name,
                                                                                                           Members = proj.ProjectMembers.Select(member => new
                                                                                                           {
                                                                                                               member.UserId,
                                                                                                               member.User.UserName,
                                                                                                               member.Role
                                                                                                           })
                                                                                                       }).FirstOrDefaultAsync(proj => proj.Id == id));
    }
}
