
namespace Jira.DTOs.Comment
{
    public class GetCommentsDto
    {
        public required string TaskItemId { get; set; }
        public required List<GetCommentDto> Comments { get; set; }
    }
}
