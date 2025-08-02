
namespace Jira.Models.Entities
{
    public class Column
    {
        public string Id { get; set; }
        public required string Name { get; set; }
        public required string BoardId { get; set; }
        public Board? Board { get; set; }
        public int Position { get; set; }
        public required List<TaskItem> Tasks { get; set; } = [];
    }
}
