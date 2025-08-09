namespace Jira.DTOs.Board
{
    public class GetBoardsDto
    {
        public string ProjectName { get; set; }
        public List<GetBoardDto> Boards { get; set; }
    }
}
