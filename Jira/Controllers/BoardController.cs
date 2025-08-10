using Jira.Data;
using Jira.DTOs.Board;
using Jira.Infrastructure;
using Jira.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

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
            var check = await AppDbContext.CheckForNull(projectId: projectId);

            if (check != null)
            {
                return NotFound(check);
            }

            if (!await AppDbContext.IsRequiredOrAdmin(projectId, User, Constants.AllMembers))
            {
                return Forbid();
            }

            var board = new Board()
            {
                Id = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                Name = dto.Name,
                ProjectId = projectId
            };

            await AppDbContext.Boards.AddAsync(board);

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
            var check = await AppDbContext.CheckForNull(projectId: projectId);

            if (check != null)
            {
                return NotFound(check);
            }

            if (!await AppDbContext.IsRequiredOrAdmin(projectId, User, Constants.AllMembers))
            {
                return Forbid();
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
            var check = await AppDbContext.CheckForNull(projectId: projectId, boardIds: [boardId]);

            if (check != null)
            {
                return NotFound(check);
            }

            if (!await AppDbContext.IsRequiredOrAdmin(projectId, User, Constants.AllMembers))
            {
                return Forbid();
            }

            var board = await AppDbContext.Boards.FirstOrDefaultAsync(board => board.Id == boardId);

            if (dto.Name != null)
            {
                board.Name = dto.Name;
                board.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                return BadRequest();
            }

            await AppDbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{projectId}/boards/{boardId}")]
        public async Task<IActionResult> DeleteBoard([FromRoute] string boardId,
                                                     [FromRoute] string projectId)
        {
            var check = await AppDbContext.CheckForNull(projectId: projectId, boardIds: [boardId]);

            if (check != null)
            {
                return NotFound(check);
            }

            if (!await AppDbContext.IsRequiredOrAdmin(projectId, User, Constants.OwnerAndManagers))
            {
                return Forbid();
            }

            var board = await AppDbContext.Boards.FirstOrDefaultAsync(board => board.Id == boardId);

            AppDbContext.Boards.Remove(board);
            await AppDbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
