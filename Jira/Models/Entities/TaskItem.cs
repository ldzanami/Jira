namespace Jira.Models.Entities
{
    public class TaskItem
    {
        public string Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public required string ColumnId { get; set; }
        public string? AssignedId { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public List<Tag>? Tags { get; set; } = [];
        public List<Comment>? Comments { get; set; } = [];
        public Column? Column { get; set; }
        public User? Assigned { get; set; }
    }
}
