namespace Jira.DTOs.Project
{
    public class CreateDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
    }
}
