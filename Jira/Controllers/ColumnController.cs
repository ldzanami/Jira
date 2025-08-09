using Jira.Data;
using Jira.DTOs.Column;
using Jira.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jira.Controllers
{
    [ApiController]
    [Route("api/project")]
    [Authorize]
    public class ColumnController(AppDbContext appDbContext) : ControllerBase
    {
        private AppDbContext AppDbContext { get; set; } = appDbContext;

        [HttpPost("{projectId}/board/{boardId}/columns")]
        public async Task<IActionResult> CreateColumn([FromRoute] string projectId,
                                                      [FromRoute] string boardId,
                                                      [FromBody] CreateColumnDto dto)
        {
            var project = await AppDbContext.Projects.Where(proj => proj.Id == projectId)
                                                     .Include(proj => proj.Boards)
                                                     .ThenInclude(board => board.Columns)
                                                     .SingleOrDefaultAsync();

            if (project == null)
            {
                return NotFound();
            }

            var board = project.Boards.FirstOrDefault(board => board.Id == boardId);

            if (board == null)
            {
                return NotFound();
            }
            if(board.Columns.OrderBy(column => column.Position).Last() != null)
            {
                if (dto.Position < board.Columns.OrderBy(column => column.Position).Last().Position)
                {
                    var columnFromPosition = board.Columns.FirstOrDefault(column => column.Position == dto.Position);

                    if (columnFromPosition != null)
                    {
                        foreach (var i in board.Columns.Where(column => column.Position >= dto.Position))
                        {
                            i.Position++;
                        }
                    }
                }
                else
                {
                    dto.Position = board.Columns.OrderBy(column => column.Position).Last().Position + 1;
                }
            }
            else
            {
                dto.Position = 0;
            }

            var column = new Column
            {
                BoardId = boardId,
                Id = Guid.NewGuid().ToString(),
                Name = dto.Name,
                Position = dto.Position
            };

            board.Columns.Add(column);
            await AppDbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(CreateColumn), new GetColumnDto
            {
                Id = column.Id,
                Name = column.Name,
                Position = column.Position
            });
        }

        [HttpGet("{projectId}/board/{boardId}/columns")]
        public async Task<IActionResult> GetAllColumnsInBoard([FromRoute] string projectId,
                                                              [FromRoute] string boardId)
        {
            var project = await AppDbContext.Projects.Where(proj => proj.Id == projectId)
                                                     .Include(proj => proj.Boards)
                                                     .ThenInclude(board => board.Columns)
                                                     .SingleOrDefaultAsync();

            if (project == null)
            {
                return NotFound();
            }

            var board = project.Boards.FirstOrDefault(board => board.Id == boardId);

            if (board == null)
            {
                return NotFound();
            }

            return Ok(new GetColumnsDto
            {
                BoardId = boardId,
                Columns = board.Columns.OrderBy(column => column.Position)
                                       .Select(column => new GetColumnDto
                                       {
                                           Id = column.Id,
                                           Name = column.Name,
                                           Position = column.Position
                                       }).ToList()
            });
        }

        [HttpPatch("{projectId}/board/{boardId}/columns/{columnId}")]
        public async Task<IActionResult> UpdateColumn([FromRoute] string projectId,
                                                      [FromRoute] string boardId,
                                                      [FromRoute] string columnId,
                                                      [FromBody] UpdateColumnDto dto)
        {
            var project = await AppDbContext.Projects.Where(proj => proj.Id == projectId)
                                                     .Include(proj => proj.Boards)
                                                     .ThenInclude(board => board.Columns)
                                                     .SingleOrDefaultAsync();

            if (project == null)
            {
                return NotFound();
            }

            var board = project.Boards.FirstOrDefault(board => board.Id == boardId);

            if (board == null)
            {
                return NotFound();
            }

            var column = board.Columns.FirstOrDefault(column => column.Id == columnId);

            if(column == null)
            {
                return NotFound();
            }
            if (board.Columns.OrderBy(column => column.Position).Last() != null)
            {
                if (dto.Position < board.Columns.OrderBy(column => column.Position).Last().Position)
                {
                    var columnFromPosition = board.Columns.FirstOrDefault(column => column.Position == dto.Position);

                    if (columnFromPosition != null)
                    {
                        if (dto.Position < column.Position)
                        {
                            foreach (var i in board.Columns.Where(c => c.Position >= dto.Position && c.Position < column.Position))
                            {
                                i.Position++;
                            }
                        }
                        else if (dto.Position > column.Position)
                        {
                            foreach (var i in board.Columns.Where(c => c.Position <= dto.Position && c.Position > column.Position))
                            {
                                i.Position--;
                            }
                        }
                    }
                }
                else
                {
                    dto.Position = board.Columns.OrderBy(column => column.Position).Last().Position + 1;
                }
            }
            else
            {
                dto.Position = 0;
            }
            

            column.Position = dto.Position;
            column.Name = dto.Name;
            await AppDbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{projectId}/board/{boardId}/columns/{columnId}")]
        public async Task<IActionResult> DeleteColumn([FromRoute] string projectId,
                                                      [FromRoute] string boardId,
                                                      [FromRoute] string columnId)
        {
            var project = await AppDbContext.Projects.Where(proj => proj.Id == projectId)
                                                     .Include(proj => proj.Boards)
                                                     .ThenInclude(board => board.Columns)
                                                     .Include(proj => proj.ProjectMembers)
                                                     .ThenInclude(member => member.User)
                                                     .SingleOrDefaultAsync();

            if (project == null)
            {
                return NotFound();
            }

            var board = project.Boards.FirstOrDefault(board => board.Id == boardId);

            if (board == null)
            {
                return NotFound();
            }

            var column = board.Columns.FirstOrDefault(column => column.Id == columnId);

            if (column == null)
            {
                return NotFound();
            }

            var me = project.ProjectMembers.Where(member => member.User.UserName == User.Identity.Name)
                                           .SingleOrDefault();

            if (me.Role != "Admin" && me.Role != "Owner" && me.User.Role != "Admin")
            {
                return Forbid();
            }

            board.Columns.Remove(column);
            await AppDbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
