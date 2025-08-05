namespace Jira.DTOs.Tag
{
    public class GetTagsDto
    {
        public required string TaskItemId { get; set; }
        public required List<GetTagDto> Tags { get; set; }
    }
}
