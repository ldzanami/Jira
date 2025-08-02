using Jira.Data;
using Jira.DTOs.Project;
using Jira.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jira.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController(AppDbContext appDbContext, SignInManager<User> signInManager) : ControllerBase
    {
        private AppDbContext AppDbContext { get; set; } = appDbContext;
        private SignInManager<User> SignInManager { get; set; } = signInManager;
             
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateDto createDto)
        {
            if (AppDbContext.Projects.Where(proj => proj.Name == createDto.Name).Any())
            {
                return BadRequest(new { Error = "Проект с таким именем уже есть" });
            }
                var project = new Project()
            {
                Id = Guid.NewGuid().ToString(),
                Name = createDto.Name,
                Description = createDto.Description,
                OwnerId = SignInManager.UserManager.GetUserId(User),
                CreatedAt = DateTime.UtcNow,
                Boards = [],
                ProjectMembers = []
            };
            await AppDbContext.Projects.AddAsync(project);
            await AppDbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(CreateProject), new ReturnableCreatedDto()
            {
                Id = project.Id,
                Description = project.Description,
                Name = project.Name,
                CreatedAt = project.CreatedAt,
                OwnerId = SignInManager.UserManager.GetUserId(User),
                OwnerName = SignInManager.UserManager.GetUserName(User)
            });
        }
    }
}
