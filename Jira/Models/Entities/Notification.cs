namespace Jira.Models.Entities
{
    public class Notification
    {
        public string Id { get; set; }
        public required string UserId { get; set; }
        public User? User { get; set; }
        public required string Type { get; set; }
        public required string Message { get; set; }
        public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public required bool IsRead { get; set; } = false;
        public required string RelatedTaskId { get; set; }
        public TaskItem? RelatedTask { get; set; }
    }
}
