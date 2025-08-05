namespace Jira.DTOs.Comment
{
    public class GetCommentDto
    {
        public required string Id { get; set; }
        public required string Text { get; set; }
        public required DateTime UpdatedAt { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required string AuthorId { get; set; }
        public required string AuthorName { get; set; }
    }
}
