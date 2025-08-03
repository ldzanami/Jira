using Microsoft.EntityFrameworkCore;

namespace Jira.Models.Entities
{
    [PrimaryKey(nameof(UserId), nameof(ProjectId))]
    public class ProjectMember
    {
        public required string UserId { get; set; }
        public required string ProjectId { get; set; }
        public User? User { get; set; }
        public Project? Project { get; set; }
        public required string Role { get; set; }
    }
}
