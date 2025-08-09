using Jira.Data;
using Jira.DTOs.TaskItem;
using Jira.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jira.Controllers
{
    [ApiController]
    [Route("api/project")]
    [Authorize]
    public class TaskItemController(AppDbContext appDbContext) : ControllerBase
    {
        private AppDbContext AppDbContext { get; set; } = appDbContext;

        [HttpPost("{projectId}/board/{boardId}/column/{columnId}/tasks")]
        public async Task<IActionResult> CreateTask([FromRoute] string projectId,
                                                    [FromRoute] string boardId,
                                                    [FromRoute] string columnId,
                                                    [FromBody] CreateTaskDto dto)
        {
            var project = await AppDbContext.Projects.Where(proj => proj.Id == projectId)
                                                     .Include(proj => proj.Boards)
                                                     .ThenInclude(board => board.Columns)
                                                     .ThenInclude(column => column.Tasks)
                                                     .SingleOrDefaultAsync();

            if (project == null)
            {
                return NotFound();
            }

            var board = project.Boards.FirstOrDefault(board => board.Id == boardId);

            if(board == null)
            {
                return NotFound();
            }

            var column = board.Columns.FirstOrDefault(column => column.Id == columnId);

            if(column == null)
            {
                return NotFound();
            }

            var task = new TaskItem
            {
                Title = dto.Title,
                ColumnId = columnId,
                Id = Guid.NewGuid().ToString(),
                Description = dto.Description,
                Priority = dto.Priority,
                DueDate = dto.DueDate,
                Status = dto.Status,
                Tags = dto.Tags,
                CreatedAt = DateTime.UtcNow
            };

            column.Tasks.Add(task);
            await AppDbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(CreateTask), new GetTaskDto
            {
                AssignedId = task.AssignedId,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                Description = task.Description,
                DueDate = task.DueDate,
                Id = task.Id,
                Priority = task.Priority,
                Status = task.Status,
                Title = task.Title,
                Tags = task.Tags
            });
        }

        [HttpGet("{projectId}/board/{boardId}/column/{columnId}/tasks")]
        public async Task<IActionResult> GetTasksInColumn([FromRoute] string projectId,
                                                          [FromRoute] string boardId,
                                                          [FromRoute] string columnId)
        {
            var project = await AppDbContext.Projects.Where(proj => proj.Id == projectId)
                                                     .Include(proj => proj.Boards)
                                                     .ThenInclude(board => board.Columns)
                                                     .ThenInclude(column => column.Tasks)
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

            return Ok(new GetTasksDto
            {
                ColumnId = column.Id,
                Tasks = column.Tasks.Select(task => new GetTaskDto
                {
                    Id = task.Id,
                    AssignedId = task.AssignedId,
                    CreatedAt = task.CreatedAt,
                    UpdatedAt = task.UpdatedAt,
                    Description = task.Description,
                    DueDate = task.DueDate,
                    Priority = task.Priority,
                    Status = task.Status,
                    Tags = task.Tags,
                    Title = task.Title
                }).ToList()
            });
        }

        [HttpPatch("{projectId}/board/{boardId}/column/{columnId}/tasks/{taskId}")]
        public async Task<IActionResult> UpdateTask([FromRoute] string projectId,
                                                    [FromRoute] string boardId,
                                                    [FromRoute] string columnId,
                                                    [FromRoute] string taskId,
                                                    [FromBody] UpdateTaskDto dto)
        {
            var project = await AppDbContext.Projects.Where(proj => proj.Id == projectId)
                                                     .Include(proj => proj.Boards)
                                                     .ThenInclude(board => board.Columns)
                                                     .ThenInclude(column => column.Tasks)
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

            var task = column.Tasks.FirstOrDefault(task => task.Id == taskId);

            if(task == null)
            {
                return NotFound();
            }

            var me = project.ProjectMembers.Where(member => member.User.UserName == User.Identity.Name)
                                           .SingleOrDefault();

            if (me.Role != "Admin" && me.Role != "Owner" && me.User.Role != "Admin")
            {
                return Forbid();
            }

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.Status = dto.Status;
            task.Priority = dto.Priority;
            task.DueDate = dto.DueDate;
            task.Tags = dto.Tags;
            task.UpdatedAt = DateTime.UtcNow;

            await AppDbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{projectId}/board/{boardId}/column/{columnId}/tasks/{taskId}")]
        public async Task<IActionResult> DeleteTask([FromRoute] string projectId,
                                                    [FromRoute] string boardId,
                                                    [FromRoute] string columnId,
                                                    [FromRoute] string taskId)
        {
            var project = await AppDbContext.Projects.Where(proj => proj.Id == projectId)
                                                     .Include(proj => proj.Boards)
                                                     .ThenInclude(board => board.Columns)
                                                     .ThenInclude(column => column.Tasks)
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

            var task = column.Tasks.FirstOrDefault(task => task.Id == taskId);

            if (task == null)
            {
                return NotFound();
            }

            var me = project.ProjectMembers.Where(member => member.User.UserName == User.Identity.Name)
                                           .SingleOrDefault();

            if (me.Role != "Admin" && me.Role != "Owner" && me.User.Role != "Admin")
            {
                return Forbid();
            }

            column.Tasks.Remove(task);
            await AppDbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{projectId}/board/{boardId}/column/{columnId}/tasks/{taskId}/move")]
        public async Task<IActionResult> MoveTaskToDifferentColumn([FromRoute] string projectId,
                                                                   [FromRoute] string boardId,
                                                                   [FromRoute] string columnId,
                                                                   [FromRoute] string taskId,
                                                                   [FromBody] MoveTaskDto dto)
        {
            var project = await AppDbContext.Projects.Where(proj => proj.Id == projectId)
                                                     .Include(proj => proj.Boards)
                                                     .ThenInclude(board => board.Columns)
                                                     .ThenInclude(column => column.Tasks)
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

            var task = column.Tasks.FirstOrDefault(task => task.Id == taskId);

            if (task == null)
            {
                return NotFound();
            }

            var newColumn = board.Columns.FirstOrDefault(column => column.Id == dto.NewColumnid);

            if(newColumn == null)
            {
                return NotFound();
            }

            var me = project.ProjectMembers.Where(member => member.User.UserName == User.Identity.Name)
                                           .SingleOrDefault();

            if (me.Role != "Admin" && me.Role != "Owner" && me.User.Role != "Admin")
            {
                return Forbid();
            }

            task.ColumnId = dto.NewColumnid;
            await AppDbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{projectId}/board/{boardId}/column/{columnId}/tasks/{taskId}/assign")]
        public async Task<IActionResult> AssignTask([FromRoute] string projectId,
                                                    [FromRoute] string boardId,
                                                    [FromRoute] string columnId,
                                                    [FromRoute] string taskId,
                                                    [FromBody] AssignTaskDto dto)
        {
            var project = await AppDbContext.Projects.Where(proj => proj.Id == projectId)
                                                     .Include(proj => proj.ProjectMembers)
                                                     .Include(proj => proj.Boards)
                                                     .ThenInclude(board => board.Columns)
                                                     .ThenInclude(column => column.Tasks)
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

            var task = column.Tasks.FirstOrDefault(task => task.Id == taskId);

            if (task == null)
            {
                return NotFound();
            }

            var assign = project.ProjectMembers.FirstOrDefault(member => member.UserId == dto.AssignedId);

            if(assign == null)
            {
                return NotFound();
            }

            task.AssignedId = dto.AssignedId;
            await AppDbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
