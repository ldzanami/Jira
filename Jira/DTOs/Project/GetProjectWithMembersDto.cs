
namespace Jira.DTOs.Project
{
    public class GetProjectWithMembersDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<GetMemberDto> ProjectMembers { get; set; }
    }
}
