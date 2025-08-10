using Jira.Data;
using Jira.DTOs.Project;
using Jira.Infrastructure;
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
    public class ProjectController(AppDbContext appDbContext, SignInManager<User> signInManager) : ControllerBase
    {
        private AppDbContext AppDbContext { get; init; } = appDbContext;
        private SignInManager<User> SignInManager { get; init; } = signInManager;
             
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
                OwnerId = (await SignInManager.UserManager.FindByNameAsync(User.Identity.Name)).Id,
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
                OwnerId = (await SignInManager.UserManager.FindByNameAsync(User.Identity.Name)).Id,
                OwnerName = User.Identity.Name
            });
        }

        [HttpGet("{id}/members")]
        public async Task<IActionResult> GetProjectMembers([FromRoute] string id)
        {
            var check = await AppDbContext.CheckForNull(projectId: id);

            if (check != null)
            {
                return NotFound(check);
            }

            return Ok(await AppDbContext.Projects.Include(proj => proj.ProjectMembers)
                                                 .ThenInclude(member => member.User)
                                                 .Select(proj => new GetProjectWithMembersDto
                                                 {
                                                     Id = proj.Id,
                                                     Name = proj.Name,
                                                     Description = proj.Description,
                                                     ProjectMembers = proj.ProjectMembers.Select(member => new GetMemberDto
                                                     {
                                                         UserId = member.UserId,
                                                         UserName = member.User.UserName,
                                                         Role = member.Role

                                                     }).ToList()
                                                 })
                                                 .FirstOrDefaultAsync(proj => proj.Id == id));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectFromId([FromRoute] string id)
        {
            var check = await AppDbContext.CheckForNull(projectId: id);

            if (check != null)
            {
                return NotFound(check);
            }
            
            var project = await AppDbContext.Projects.Where(proj => proj.Id == id).Include(proj => proj.Owner).FirstOrDefaultAsync();

            return Ok(new GetProjectDto
            {
                Id = project.Id,
                CreatedAt = project.CreatedAt,
                OwnerId = project.OwnerId,
                OwnerName = project.Owner.UserName,
                Description = project.Description,
                UpdatedAt = project.UpdatedAt,
                Name = project.Name
            });
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetAllMyProjects()
        {
            var myProjects = await AppDbContext.ProjectMembers.Include(member => member.Project)
                                                              .Include(member => member.User)
                                                              .Where(member => member.User.UserName == User.Identity.Name)
                                                              .Select(member => new GetProjectsDto
                                                              {
                                                                  Id = member.ProjectId,
                                                                  Name = member.Project.Name,
                                                                  Description = member.Project.Description,
                                                                  OwnerId = member.Project.OwnerId,
                                                                  OwnerName = member.Project.Owner.UserName,
                                                                  CreatedAt = member.Project.CreatedAt,
                                                                  UpdatedAt = member.Project.UpdatedAt
                                                              }).ToListAsync();
            if(myProjects.Count == 0)
            {
                return NotFound();
            }
            return Ok(myProjects);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateProject([FromRoute] string id, [FromBody] CreateDto dto)
        {

            var check = await AppDbContext.CheckForNull(projectId: id);

            if (check != null)
            {
                return NotFound(check);
            }

            if (!await AppDbContext.IsRequiredOrAdmin(id, User, Constants.OwnerOnly))
            {
                return Forbid();
            }

            if (AppDbContext.Projects.Where(proj => proj.Name == dto.Name).Any())
            {
                return BadRequest(new { Error = "Проект с таким именем уже есть" });
            }

            var project = await AppDbContext.Projects.FirstOrDefaultAsync(proj => proj.Id == id);

            if (dto.Name != null)
            {
                project.Name = dto.Name;
            }
            if (dto.Description != null)
            {
                project.Description = dto.Description;
            }
            project.UpdatedAt = DateTime.UtcNow;

            await AppDbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject([FromRoute] string id)
        {
            var check = await AppDbContext.CheckForNull(projectId: id);

            if (check != null)
            {
                return NotFound(check);
            }

            if (!await AppDbContext.IsRequiredOrAdmin(id, User, Constants.OwnerOnly))
            {
                return Forbid();
            }
            
            var project = AppDbContext.Projects.FirstOrDefault(proj => proj.Id == id);

            AppDbContext.Projects.Remove(project);
            await AppDbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{projectId}/members/{userId}")]
        public async Task<IActionResult> UpdateMemberRole([FromRoute] string projectId,
                                                          [FromRoute] string userId,
                                                          [FromBody] ChangeMemberRoleDto dto)
        {
            if (dto.Role == "Owner")
            {
                return BadRequest();
            }

            var check = await AppDbContext.CheckForNull(projectId: projectId, memberIds: [userId]);

            if (check != null)
            {
                return NotFound(check);
            }

            if (!await AppDbContext.IsRequiredOrAdmin(projectId, User, Constants.OwnerAndManagers))
            {
                return Forbid();
            }

            var member = await AppDbContext.ProjectMembers.FirstOrDefaultAsync(member => member.ProjectId == projectId && member.UserId == userId);
            
            if (dto.Role != null)
            {
                member.Role = dto.Role;
            }
            else
            {
                return BadRequest();
            }

            await AppDbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{projectId}/members/{userId}")]
        public async Task<IActionResult> DeleteMember([FromRoute] string projectId,
                                                      [FromRoute] string userId)
        {
            var check = await AppDbContext.CheckForNull(projectId: projectId, memberIds: [userId]);

            if (check != null)
            {
                return NotFound(check);
            }

            if (!await AppDbContext.IsRequiredOrAdmin(projectId, User, Constants.OwnerAndManagers))
            {
                return Forbid();
            }

            var member = await AppDbContext.ProjectMembers.FirstOrDefaultAsync(member => member.UserId == userId && member.ProjectId == projectId);

            AppDbContext.ProjectMembers.Remove(member);
            await AppDbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/members")]
        public async Task<IActionResult> AddProjectMember([FromRoute] string id, [FromBody] AddMemberDto dto)
        {
            if (dto.Role == "Owner")
            {
                return BadRequest();
            }

            var check = await AppDbContext.CheckForNull(projectId: id, userIds: [dto.Id]);

            if (check != null)
            {
                return NotFound(check);
            }

            if (!await AppDbContext.IsRequiredOrAdmin(id, User, Constants.OwnerAndManagers))
            {
                return Forbid();
            }

            var projectMember = await AppDbContext.ProjectMembers.FirstOrDefaultAsync(member => member.UserId == dto.Id && member.ProjectId == id);

            if (projectMember != null)
            {
                return BadRequest(new { Error = "Пользователь уже находится в этом проекте" });
            }

            await AppDbContext.ProjectMembers.AddAsync(new ProjectMember
            {
                ProjectId = id,
                UserId = dto.Id,
                Role = dto.Role
            });
            await AppDbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
