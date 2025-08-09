using Jira.Models.Entities;

namespace Jira.DTOs.TaskItem
{
    public class CreateTaskDto
    {
        public required string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public DateTime DueDate { get; set; }
        public List<string> Tags { get; set; } = [];
    }
}
