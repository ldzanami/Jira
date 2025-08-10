using Jira.Data;
using Jira.DTOs.TaskItem;
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
    public class TaskItemController(AppDbContext appDbContext) : ControllerBase
    {
        private AppDbContext AppDbContext { get; set; } = appDbContext;

        [HttpPost("{projectId}/board/{boardId}/column/{columnId}/tasks")]
        public async Task<IActionResult> CreateTask([FromRoute] string projectId,
                                                    [FromRoute] string boardId,
                                                    [FromRoute] string columnId,
                                                    [FromBody] CreateTaskDto dto)
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

            await AppDbContext.TaskItems.AddAsync(task);
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
            var check = await AppDbContext.CheckForNull(projectId: projectId, boardIds: [boardId], columnIds: [columnId]);

            if (check != null)
            {
                return NotFound(check);
            }

            if (!await AppDbContext.IsRequiredOrAdmin(projectId, User, Constants.AllMembers))
            {
                return Forbid();
            }

            var column = await AppDbContext.Columns.Include(column => column.Tasks)
                                                   .FirstOrDefaultAsync(column => column.Id == columnId);

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
            var check = await AppDbContext.CheckForNull(projectId: projectId, boardIds: [boardId], columnIds: [columnId], taskIds: [taskId]);

            if (check != null)
            {
                return NotFound(check);
            }

            if (!await AppDbContext.IsRequiredOrAdmin(projectId, User, Constants.AllMembers))
            {
                return Forbid();
            }

            var task = await AppDbContext.TaskItems.FirstOrDefaultAsync(task => task.Id == taskId);

            if (dto.Title != null)
            {
                task.Title = dto.Title;
            }
            if(dto.Description != null)
            {
                task.Description = dto.Description;
            }
            if (dto.Status != null)
            {
                task.Status = dto.Status;
            }
            if (dto.Priority != null)
            {
                task.Priority = dto.Priority;
            }
            if (dto.DueDate != null)
            {
                task.DueDate = dto.DueDate;
            }
            if (dto.Tags != null)
            {
                task.Tags = dto.Tags;
            }
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
            var check = await AppDbContext.CheckForNull(projectId: projectId, boardIds: [boardId], columnIds: [columnId], taskIds: [taskId]);

            if (check != null)
            {
                return NotFound(check);
            }

            if (!await AppDbContext.IsRequiredOrAdmin(projectId, User, Constants.OwnerAndManagers))
            {
                return Forbid();
            }

            var task = AppDbContext.TaskItems.FirstOrDefault(task => task.Id == taskId);

            AppDbContext.TaskItems.Remove(task);
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
            var check = await AppDbContext.CheckForNull(projectId: projectId, boardIds: [boardId], columnIds: [columnId, dto.NewColumnid], taskIds: [taskId]);

            if (check != null)
            {
                return NotFound(check);
            }

            if (!await AppDbContext.IsRequiredOrAdmin(projectId, User, Constants.OwnerAndManagers))
            {
                return Forbid();
            }

            var task = AppDbContext.TaskItems.FirstOrDefault(task => task.Id == taskId);

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
            var check = await AppDbContext.CheckForNull(projectId: projectId,
                                                        boardIds: [boardId],
                                                        columnIds: [columnId],
                                                        taskIds: [taskId],
                                                        userIds: [dto.AssignedId],
                                                        memberIds: [dto.AssignedId]);

            if (check != null)
            {
                return NotFound(check);
            }

            if (!await AppDbContext.IsRequiredOrAdmin(projectId, User, Constants.AllMembers))
            {
                return Forbid();
            }

            var task = AppDbContext.TaskItems.FirstOrDefault(task => task.Id == taskId);

            task.AssignedId = dto.AssignedId;
            await AppDbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}