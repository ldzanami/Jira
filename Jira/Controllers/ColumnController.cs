using Jira.Data;
using Jira.DTOs.Column;
using Jira.Infrastructure;
using Jira.Models.Entities;
using Microsoft.AspNetCore.Authorization;
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
            var check = await AppDbContext.CheckForNull(projectId: projectId, boardIds: [boardId]);

            if (check != null)
            {
                return NotFound(check);
            }

            if (!await AppDbContext.IsRequiredOrAdmin(projectId, User, Constants.AllMembers))
            {
                return Forbid();
            }

            var board = AppDbContext.Boards.Include(board => board.Columns)
                                           .FirstOrDefault(board => board.Id == boardId);

            if (board.Columns.Count > 15)
            {
                return BadRequest(new { Error = "Максимальное число колонн = 15" });
            }

            var column = new Column
            {
                BoardId = boardId,
                Id = Guid.NewGuid().ToString(),
                Name = dto.Name,
                Position = dto.Position
            };

            Console.WriteLine(column.Position);
            Console.WriteLine(board.Columns.Count);

            if (column.Position < board.Columns.Count)
            {
                board.Columns.Insert(column.Position, column);
            }
            else
            {
                board.Columns.Add(column);
            }

            board.Columns = RepositionColumns(board.Columns);
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
            var check = await AppDbContext.CheckForNull(projectId: projectId, boardIds: [boardId]);

            if (check != null)
            {
                return NotFound(check);
            }

            if (!await AppDbContext.IsRequiredOrAdmin(projectId, User, Constants.AllMembers))
            {
                return Forbid();
            }

            var board = AppDbContext.Boards.Include(board => board.Columns)
                                           .FirstOrDefault(board => board.Id == boardId);

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
            var check = await AppDbContext.CheckForNull(projectId: projectId, boardIds: [boardId], columnIds: [columnId]);

            if (check != null)
            {
                return NotFound(check);
            }

            if(!await AppDbContext.IsRequiredOrAdmin(projectId, User, Constants.AllMembers))
            {
                return Forbid();
            }

            var board = await AppDbContext.Boards.Include(board => board.Columns)
                                                 .FirstOrDefaultAsync(board => board.Id == boardId);

            var column = board.Columns.FirstOrDefault(column => column.Id == columnId);

            if (dto.Position != -1)
            {
                column.Position = dto.Position;
            }

            if (dto.Name != null)
            {
                column.Name = dto.Name;
            }

            if (dto.Position > 15)
            {
                dto.Position = 16;
            }

            board.Columns.Remove(column);

            if (column.Position < board.Columns.Count)
            {
                board.Columns.Insert(column.Position, column);
            }
            else
            {
                board.Columns.Add(column);
            }

            board.Columns = RepositionColumns(board.Columns);
            await AppDbContext.SaveChangesAsync();
            return NoContent();
        }


        [HttpDelete("{projectId}/board/{boardId}/columns/{columnId}")]
        public async Task<IActionResult> DeleteColumn([FromRoute] string projectId,
                                                      [FromRoute] string boardId,
                                                      [FromRoute] string columnId)
        {
            var check = await AppDbContext.CheckForNull(projectId: projectId, columnIds: [columnId], boardIds: [boardId]);

            if(check != null)
            {
                return NotFound(check);
            }

            if (!await AppDbContext.IsRequiredOrAdmin(projectId, User, Constants.OwnerAndManagers))
            {
                return Forbid();
            }

            var board = AppDbContext.Boards.Include(board => board.Columns)
                                           .FirstOrDefault(board => board.Id == boardId);

            var column = board.Columns.FirstOrDefault(column => column.Id == columnId);

            board.Columns.Remove(column);
            board.Columns = RepositionColumns(board.Columns);
            await AppDbContext.SaveChangesAsync();
            return NoContent();
        }


        [ApiExplorerSettings(IgnoreApi = true)]
        public List<Column> RepositionColumns(List<Column> columns)
        {
            columns = CompressList(columns);
            for(int i = 0; i < columns.Count; i++)
            {
                columns[i].Position = i;
            }
            return columns;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public List<Column> CompressList(List<Column> columns) => columns.FindAll(column => column != null).ToList();
    }
}
