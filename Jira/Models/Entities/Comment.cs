namespace Jira.Models.Entities
{
    public class Comment
    {
        public string Id { get; set; }
        public required string Text { get; set; }
        public required string AuthorId { get; set; }
        public required string AuthorName { get; set; }
        public User? Author { get; set; }
        public required string TaskItemId { get; set; }
        public TaskItem? TaskItem { get; set; }
        public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }
}
