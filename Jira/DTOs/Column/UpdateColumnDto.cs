namespace Jira.DTOs.Column
{
    public class UpdateColumnDto
    {
        public string? Name { get; set; }
        public int Position { get; set; } = -1;
    }
}
