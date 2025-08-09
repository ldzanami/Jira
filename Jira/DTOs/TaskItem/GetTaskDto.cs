namespace Jira.DTOs.TaskItem
{
    public class GetTaskDto
    {
        public required string Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public string? AssignedId { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<string> Tags { get; set; } = [];
    }
}
