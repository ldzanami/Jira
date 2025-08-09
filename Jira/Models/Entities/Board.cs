namespace Jira.Models.Entities
{
    public class Board
    {
        public string Id { get; set; }
        public required string Name { get; set; }
        public required string ProjectId { get; set; }
        public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
        public Project? Project { get; set; }
        public List<Column> Columns { get; set; } = [];
    }
}
