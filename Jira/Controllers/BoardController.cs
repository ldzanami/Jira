using Jira.Data;
using Jira.DTOs.Board;
using Jira.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Jira.Controllers
{
    [ApiController]
    [Route("api/project")]
    [Authorize]
    public class BoardController(AppDbContext appDbContext) : ControllerBase
    {
        private AppDbContext AppDbContext { get; set; } = appDbContext;

        [HttpPost("{projectId}/boards")]
        public async Task<IActionResult> CreateBoard([FromRoute] string projectId, [FromBody] CreateBoardDto dto)
        {
            var project = await AppDbContext.Projects.Where(proj => proj.Id == projectId)
                                                     .Include(proj => proj.Boards)
                                                     .SingleOrDefaultAsync();

            if(project == null)
            {
                return NotFound();
            }

            var board = new Board() { Id = Guid.NewGuid().ToString(), CreatedAt = DateTime.UtcNow, Name = dto.Name, ProjectId = project.Id };

            project.Boards.Add(board);

            await AppDbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(CreateBoard), new GetBoardDto
            {
                Id = board.Id,
                Name = board.Name,
                CreatedAt = board.CreatedAt
            });
        }

        [HttpGet("{projectId}/boards")]
        public async Task<IActionResult> GetAllBoards([FromRoute] string projectId)
        {
            var project = await AppDbContext.Projects.Where(proj => proj.Id == projectId)
                                                     .SingleOrDefaultAsync();
            
            if(project == null)
            {
                return NotFound();
            }

            return Ok(AppDbContext.Projects.Where(proj => proj.Id == projectId)
                                           .Include(proj => proj.Boards)
                                           .Select(proj => new GetBoardsDto
                                           {
                                               ProjectName = proj.Name,
                                               Boards = proj.Boards.Select(board => new GetBoardDto
                                               {
                                                   Id = board.Id,
                                                   CreatedAt = board.CreatedAt,
                                                   Name = board.Name
                                               }).ToList()
                                           }));
        }

        [HttpPatch("{projectId}/boards/{boardId}")]
        public async Task<IActionResult> UpdateBoard([FromRoute] string boardId,
                                                     [FromRoute] string projectId,
                                                     [FromBody] UpdateBoardDto dto)
        {
            var project = await AppDbContext.Projects.Where(proj => proj.Id == projectId)
                                                     .Include(proj => proj.Boards)
                                                     .SingleOrDefaultAsync();

            if(project == null)
            {
                return NotFound();
            }

            var board = project.Boards.Where(board => board.Id == boardId)
                                      .SingleOrDefault();

            if(board == null)
            {
                return NotFound();
            }

            board.Name = dto.Name;
            board.UpdatedAt = DateTime.UtcNow;
            await AppDbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{projectId}/boards/{boardId}")]
        public async Task<IActionResult> DeleteBoard([FromRoute] string boardId,
                                                     [FromRoute] string projectId)
        {
            var project = await AppDbContext.Projects.Where(proj => proj.Id == projectId)
                                                     .Include(proj => proj.Owner)
                                                     .Include(proj => proj.Boards)
                                                     .SingleOrDefaultAsync();

            if (project == null)
            {
                return NotFound();
            }

            var board = project.Boards.Where(board => board.Id == boardId)
                                      .SingleOrDefault();

            if (board == null)
            {
                return NotFound();
            }

            if (!(User.Claims.Where(claim => claim.Type == ClaimTypes.Role).SingleOrDefault().Value == "Admin")
              && !(project.Owner.UserName == User.Identity.Name))
            {
                return Forbid();
            }

            project.Boards.Remove(board);
            await AppDbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
