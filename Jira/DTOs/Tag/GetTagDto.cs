
namespace Jira.DTOs.Tag
{
    public class GetTagDto
    {
        public required string Id { get; set; }
        public required DateTime AddedAt { get; set; }
        public required string Name { get; set; }
    }
}
