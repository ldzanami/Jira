namespace Jira.DTOs.Column
{
    public class CreateColumnDto
    {
        public required string Name { get; set; }
        public int Position { get; set; }
    }
}
