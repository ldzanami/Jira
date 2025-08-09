
namespace Jira.Models.Entities
{
    public class Column
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public int Position { get; set; }
        public required string BoardId { get; set; }
        public Board? Board { get; set; }
        public List<TaskItem>? Tasks { get; set; } = [];
    }
}
