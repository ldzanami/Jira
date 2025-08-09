using Jira.Data;
using Jira.DTOs.Comment;
using Jira.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Jira.Controllers
{
    [ApiController]
    [Route("api/project")]
    [Authorize]
    public class CommentController(AppDbContext appDbContext, UserManager<User> userManager) : ControllerBase
    {
        private AppDbContext AppDbContext { get; set; } = appDbContext;
        private UserManager<User> UserManager { get; set; } = userManager;

        [HttpPost("{projectId}/board/{boardId}/column/{columnId}/task/{taskId}/[controller]")]
        public async Task<IActionResult> AddCommentToTaskItem([FromRoute] string projectId,
                                                              [FromRoute] string boardId,
                                                              [FromRoute] string columnId,
                                                              [FromRoute] string taskId,
                                                              [FromBody] AddCommentDto dto)
        {
            var taskItem = await AppDbContext.GetTaskItem(projectId, boardId, columnId, taskId);

            if(taskItem == null)
            {
                return NotFound(new { Error = "Такого TaskItem не существует" });
            }

            var author = await UserManager.FindByNameAsync(User.Identity.Name);

            var comment = new Comment
            {
                Id = Guid.NewGuid().ToString(),
                AuthorId = author.Id,
                CreatedAt = DateTime.UtcNow,
                TaskItemId = taskId,
                Text = dto.Text,
                AuthorName = author.UserName
            };

            taskItem.Comments.Add(comment);
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
            var taskItem = await AppDbContext.GetTaskItem(projectId, boardId, columnId, taskId);

            if (taskItem == null)
            {
                return NotFound(new { Error = "Такого TaskItem не существует" });
            }

            var comment = taskItem.Comments.FirstOrDefault(comment => comment.Id == commentId);

            if(comment == null)
            {
                return NotFound(new { Error = "Такого Comment не существует" });
            }

            if(comment.AuthorName != User.Identity.Name || !await UserManager.IsInRoleAsync(await UserManager.FindByNameAsync(User.Identity.Name), "Admin"))
            {
                return Forbid();
            }

            taskItem.Comments.Remove(comment);
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
            var taskItem = await AppDbContext.GetTaskItem(projectId, boardId, columnId, taskId);

            if (taskItem == null)
            {
                return NotFound(new { Error = "Такого TaskItem не существует" });
            }

            var comment = taskItem.Comments.FirstOrDefault(comment => comment.Id == commentId);

            if (comment == null)
            {
                return NotFound(new { Error = "Такого Comment не существует" });
            }

            if (comment.AuthorName != User.Identity.Name || !await UserManager.IsInRoleAsync(await UserManager.FindByNameAsync(User.Identity.Name), "Admin"))
            {
                return Forbid();
            }

            comment.Text = dto.Text;
            comment.UpdatedAt = DateTime.UtcNow;

            await AppDbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{projectId}/board/{boardId}/column/{columnId}/task/{taskId}/comments")]
        public async Task<IActionResult> GetComments([FromRoute] string projectId,
                                                     [FromRoute] string boardId,
                                                     [FromRoute] string columnId,
                                                     [FromRoute] string taskId)
        {
            var taskItem = await AppDbContext.GetTaskItem(projectId, boardId, columnId, taskId);

            if (taskItem == null)
            {
                return NotFound(new { Error = "Такого TaskItem не существует" });
            }

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
