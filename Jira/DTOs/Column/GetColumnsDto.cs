namespace Jira.DTOs.Column
{
    public class GetColumnsDto
    {
        public string BoardId { get; set; }
        public List<GetColumnDto> Columns { get; set; } = [];
    }
}
