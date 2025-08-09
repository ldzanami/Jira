namespace Jira.DTOs.TaskItem
{
    public class GetTasksDto
    {
        public required string ColumnId { get; set; }
        public List<GetTaskDto> Tasks { get; set; } = [];
    }
}
