
namespace Jira.DTOs.Tag
{
    public class AddTagDto
    {
        public required string Name { get; set; }
        public required string TaskItemId { get; set; }
    }
}
