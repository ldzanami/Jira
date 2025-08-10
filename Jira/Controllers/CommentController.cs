using Jira.Data;
using Jira.DTOs.Comment;
using Jira.Infrastructure;
using Jira.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using System.Security.Claims;

namespace Jira.Controllers
{
    [ApiController]
    [Route("api/project")]
    [Authorize]
    public class CommentController(AppDbContext appDbContext) : ControllerBase
    {
        private AppDbContext AppDbContext { get; set; } = appDbContext;

        [HttpPost("{projectId}/board/{boardId}/column/{columnId}/task/{taskId}/[controller]")]
        public async Task<IActionResult> AddCommentToTaskItem([FromRoute] string projectId,
                                                              [FromRoute] string boardId,
                                                              [FromRoute] string columnId,
                                                              [FromRoute] string taskId,
                                                              [FromBody] AddCommentDto dto)
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

            var comment = new Comment
            {
                Id = Guid.NewGuid().ToString(),
                AuthorId = User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Sid).Value,
                CreatedAt = DateTime.UtcNow,
                TaskItemId = taskId,
                Text = dto.Text,
                AuthorName = User.Identity.Name
            };

            await AppDbContext.Comments.AddAsync(comment);
            await AppDbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(AddCommentToTaskItem), new GetCommentDto
            {
                AuthorId = comment.AuthorId,
                AuthorName = comment.AuthorName,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                Id = comment.Id,
                Text = comment.Text
            });
        }

        [HttpDelete("{projectId}/board/{boardId}/column/{columnId}/task/{taskId}/{commentId}")]
        public async Task<IActionResult> RemoveCommentFromTaskItem([FromRoute] string projectId,
                                                                   [FromRoute] string boardId,
                                                                   [FromRoute] string columnId,
                                                                   [FromRoute] string taskId,
                                                                   [FromRoute] string commentId)
        {
            var check = await AppDbContext.CheckForNull(projectId: projectId, boardIds: [boardId], columnIds: [columnId], taskIds: [taskId], commentIds: [commentId]);

            if (check != null)
            {
                return NotFound(check);
            }

            var comment = await AppDbContext.Comments.FirstOrDefaultAsync(comment => comment.Id == commentId);

            if (!await AppDbContext.IsRequiredOrAdmin(projectId, User, Constants.OwnerOnly) && comment.AuthorName != User.Identity.Name)
            {
                return Forbid();
            }

            AppDbContext.Comments.Remove(comment);
            await AppDbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{projectId}/board/{boardId}/column/{columnId}/task/{taskId}/{commentId}")]
        public async Task<IActionResult> EditComment([FromRoute] string projectId,
                                                     [FromRoute] string boardId,
                                                     [FromRoute] string columnId,
                                                     [FromRoute] string taskId,
                                                     [FromRoute] string commentId,
                                                     [FromBody] EditCommentDto dto)
        {
            var check = await AppDbContext.CheckForNull(projectId: projectId, boardIds: [boardId], columnIds: [columnId], taskIds: [taskId], commentIds: [commentId]);

            if (check != null)
            {
                return NotFound(check);
            }

            var comment = await AppDbContext.Comments.FirstOrDefaultAsync(comment => comment.Id == commentId);

            if (!await AppDbContext.IsRequiredOrAdmin(projectId, User, Constants.OwnerOnly) && comment.AuthorName != User.Identity.Name)
            {
                return Forbid();
            }

            if (dto.Text != null)
            {
                comment.Text = dto.Text;
                comment.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                return BadRequest();
            }

            await AppDbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{projectId}/board/{boardId}/column/{columnId}/task/{taskId}/comments")]
        public async Task<IActionResult> GetComments([FromRoute] string projectId,
                                                     [FromRoute] string boardId,
                                                     [FromRoute] string columnId,
                                                     [FromRoute] string taskId)
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

            var taskItem = await AppDbContext.TaskItems.Include(task => task.Comments)
                                                       .FirstOrDefaultAsync(task => task.Id == taskId);

            return Ok(new GetCommentsDto
            {
                TaskItemId = taskId,
                Comments = taskItem.Comments.Select(comment => new GetCommentDto
                {
                    AuthorId = comment.AuthorId,
                    Text = comment.Text,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedAt = comment.CreatedAt,
                    AuthorName = comment.AuthorName,
                    Id = comment.Id
                }).ToList()
            });
        }
    }
}
