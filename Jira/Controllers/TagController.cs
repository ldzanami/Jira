using Jira.Data;
using Jira.DTOs.Tag;
using Jira.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jira.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TagController(AppDbContext appDbContext) : ControllerBase
    {
        private AppDbContext AppDbContext { get; init; } = appDbContext;

        [HttpPost]
        public async Task<IActionResult> AddTagToTaskItem([FromBody] AddTagDto dto)
        {
            var taskItem = await AppDbContext.TaskItems.Where(task => task.Id == dto.TaskItemId)
                                                       .Include(task => task.Tags)
                                                       .FirstOrDefaultAsync();

            if (taskItem == null)
            {
                return NotFound(new { Error = "Такого TaskItem нету" });
            }

            List<string> tagNames = taskItem.Tags.Select(tag => tag.Name)
                                                 .ToList();

            if (tagNames.Contains(dto.Name))
            {
                return BadRequest(new { Error = "Этот тег уже есть в TaskItem" });
            }

            var tag = new Tag
            {
                Id = Guid.NewGuid().ToString(),
                Name = dto.Name,
                TaskItemId = dto.TaskItemId,
                AddedAt = DateTime.UtcNow
            };
            await AppDbContext.Tags.AddAsync(tag);
            await AppDbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(AddTagToTaskItem), tag);
        }

        [HttpDelete("{taskItemId}/{tagName}")]
        public async Task<IActionResult> RemoveTagFromTaskItem([FromRoute] string taskItemId, [FromRoute] string tagName)
        {
            var taskItem = await AppDbContext.TaskItems.Where(task => task.Id == taskItemId)
                                                       .Include(task => task.Tags)
                                                       .FirstOrDefaultAsync();

            if (taskItem == null)
            {
                return NotFound(new { Error = "Такого TaskItem нету" });
            }

            var tag = taskItem.Tags.Where(tag => tag.Name == tagName)
                                   .FirstOrDefault();
            if (tag == null)
            {
                return NotFound(new { Error = "Такого Tag нету" });
            }
            taskItem.Tags.Remove(tag);
            await AppDbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{taskItemId}/tags")]
        public async Task<IActionResult> GetTags([FromRoute] string taskItemId)
        {
            var taskItem = await AppDbContext.TaskItems.Where(task => task.Id == taskItemId)
                                                       .Include(task => task.Tags)
                                                       .FirstOrDefaultAsync();

            if (taskItem == null)
            {
                return NotFound(new { Error = "Такого TaskItem нету" });
            }

            return Ok(new GetTagsDto
            {
                TaskItemId = taskItemId,
                Tags = taskItem.Tags.Select(tag => new GetTagDto
                {
                    AddedAt = tag.AddedAt,
                    Id = tag.Id,
                    Name = tag.Name
                }).ToList()
            });
        }

    }
}
