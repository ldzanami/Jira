using Jira.Models.Entities;

namespace Jira.DTOs.Comment
{
    public class AddCommentDto
    {
        public required string Text { get; set; }
        public required string TaskItemId { get; set; }
    }
}
