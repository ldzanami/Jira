using Jira.Data;
using Jira.DTOs.Comment;
using Jira.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;

namespace Jira.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CommentController(AppDbContext appDbContext, UserManager<User> userManager) : ControllerBase
    {
        private AppDbContext AppDbContext { get; set; } = appDbContext;
        private UserManager<User> UserManager { get; set; } = userManager;

        [HttpPost]
        public async Task<IActionResult> AddCommentToTaskItem([FromBody] AddCommentDto dto)
        {
            var taskItem = await AppDbContext.TaskItems.Where(task => task.Id == dto.TaskItemId)
                                                       .Include(task => task.Comments)
                                                       .ThenInclude(comment => comment.Author)
                                                       .FirstOrDefaultAsync();

            if(taskItem == null)
            {
                return NotFound(new { Error = "Такого TaskItem не существует" });
            }

            var author = await UserManager.FindByNameAsync(User.Identity.Name);

            var comment = new Comment
            {
                AuthorId = author.Id,
                CreatedAt = DateTime.UtcNow,
                TaskItemId = dto.TaskItemId,
                Text = dto.Text,
                AuthorName = author.UserName
            };

            taskItem.Comments.Add(comment);
            await AppDbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(AddCommentToTaskItem), comment);
        }

        [HttpDelete("{taskItemId}/{commentId}")]
        public async Task<IActionResult> RemoveCommentFromTaskItem([FromRoute] string taskItemId, [FromRoute] string commentId)
        {
            var taskItem = await AppDbContext.TaskItems.Where(task => task.Id == taskItemId)
                                                       .Include(task => task.Comments)
                                                       .FirstOrDefaultAsync();

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

        [HttpPatch]
        public async Task<IActionResult> EditComment([FromBody] EditCommentDto dto)
        {
            var taskItem = await AppDbContext.TaskItems.Where(task => task.Id == dto.TaskItemId)
                                                       .Include(task => task.Comments)
                                                       .FirstOrDefaultAsync();

            if (taskItem == null)
            {
                return NotFound(new { Error = "Такого TaskItem не существует" });
            }

            var comment = taskItem.Comments.FirstOrDefault(comment => comment.Id == dto.CommentId);

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

        [HttpGet("{taskItemId}/comments")]
        public async Task<IActionResult> GetComments([FromRoute] string taskItemId)
        {
            var taskItem = await AppDbContext.TaskItems.Where(task => task.Id == taskItemId)
                                                       .Include(task => task.Comments)
                                                       .ThenInclude(comment => comment.Author)
                                                       .FirstOrDefaultAsync();

            if (taskItem == null)
            {
                return NotFound(new { Error = "Такого TaskItem не существует" });
            }

            return Ok(new GetCommentsDto
            {
                TaskItemId = taskItemId,
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
