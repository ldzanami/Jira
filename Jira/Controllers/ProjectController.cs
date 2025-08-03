using Jira.Data;
using Jira.DTOs.Project;
using Jira.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Jira.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController(AppDbContext appDbContext, SignInManager<User> signInManager) : ControllerBase
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
                CreatedAt = DateTime.UtcNow
            };
            await AppDbContext.ProjectMembers.AddAsync(new ProjectMember() { ProjectId = project.Id, UserId = project.OwnerId, Role = "Owner"});
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

        [HttpGet("{id}/members")]
        public async Task<IActionResult> GetProjectMembers([FromRoute] string id)
        {
            if (await AppDbContext.Projects.FindAsync(id) == null)
            {
                return NotFound();
            }
            return Ok(await AppDbContext.ProjectMembers.Where(member => member.ProjectId == id)
                                                       .Include(member => member.Project)
                                                       .Include(member => member.User)
                                                       .Select(member => new GetMemberDto()
                                                       {
                                                           UserId = member.UserId,
                                                           ProjectId = member.ProjectId,
                                                           UserName = member.User.UserName,
                                                           ProjectName = member.Project.Name,
                                                           Role = member.Role
                                                       })
                                                       .ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectFromId([FromRoute] string id)
        {
            var project = (await AppDbContext.Projects.Where(proj => proj.Id == id).Include(proj => proj.Owner).ToListAsync()).FirstOrDefault();
            if (project != null)
            {
                return Ok((ReturnableCreatedDto)project);
            }
            return NotFound();
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetAllMyProjects()
        {
            var projectOwner = await AppDbContext.Projects.Where(project => project.Owner.UserName == User.Identity.Name)
                                                          .Include(proj => proj.Owner)
                                                          .ToListAsync();

            var projectMember = await AppDbContext.ProjectMembers.Where(member => member.User.UserName == User.Identity.Name && member.Project.Owner.UserName != User.Identity.Name)
                                                                 .Include(member => member.User)
                                                                 .Include(member => member.Project)
                                                                 .Include(member => member.Project.Owner)
                                                                 .Select(member => member.Project)
                                                                 .ToListAsync();

            projectMember.AddRange(projectOwner);
            if(projectMember.Count == 0)
            {
                return NotFound();
            }
            return Ok(from project in projectMember select (ReturnableCreatedDto)project);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateProject([FromRoute] string id, [FromBody] CreateDto dto)
        {
            var project = (await AppDbContext.Projects.Where(proj => proj.Id == id).Include(proj => proj.Owner).ToListAsync()).FirstOrDefault();
            if(project == null)
            {
                return NotFound();
            }
            if(project.Owner.UserName != User.Identity.Name)
            {
                return Forbid();
            }
            if (AppDbContext.Projects.Where(proj => proj.Name == dto.Name).Any()) return BadRequest(new {Error = "Проект с таким именем уже есть"});
            project.Name = dto.Name;
            project.Description = dto.Description;
            project.UpdatedAt = DateTime.UtcNow;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            AppDbContext.Projects.Remove(await AppDbContext.Projects.FindAsync(id));
            await AppDbContext.SaveChangesAsync();
            await AppDbContext.Projects.AddAsync(project);
            await AppDbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject([FromRoute] string id)
        {
            var project = (await AppDbContext.Projects.Where(proj => proj.Id == id).Include(proj => proj.Owner).ToListAsync()).FirstOrDefault();
            if (project == null) return NotFound();
            if (project.Owner.UserName != User.Identity.Name) return Forbid();
            AppDbContext.Projects.Remove(project);
            await AppDbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/members")]
        public async Task<IActionResult> AddProjectMember([FromRoute] string id, [FromBody] AddMemberDto dto)
        {
            if ((await AppDbContext.Projects.FindAsync(id)) == null)
            {
                return NotFound();
            }
            if ((await AppDbContext.Projects.Where(proj => proj.Id == id).Include(proj => proj.Owner).ToListAsync()).FirstOrDefault().Owner.UserName != User.Identity.Name)
            {
                return Forbid();
            }
            var projectMember = await AppDbContext.ProjectMembers.Where(member => member.UserId == dto.Id && member.ProjectId == id || member.Project.OwnerId == dto.Id).Include(member => member.Project).ToListAsync();
            if (projectMember.Count != 0)
            {
                return BadRequest(new { Error = "Пользователь уже находится в этом проекте" });
            }
            await AppDbContext.ProjectMembers.AddAsync(new ProjectMember() { ProjectId = id, UserId = dto.Id, Role = dto.Role });
            await AppDbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
