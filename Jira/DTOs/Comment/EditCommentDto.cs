namespace Jira.DTOs.Comment
{
    public class EditCommentDto : AddCommentDto
    {
        public required string CommentId { get; set; }
    }
}
