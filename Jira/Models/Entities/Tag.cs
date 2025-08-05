namespace Jira.Models.Entities
{
    public class Tag
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string TaskItemId { get; set; }
        public TaskItem? TaskItem { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
