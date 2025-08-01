using Microsoft.EntityFrameworkCore;

namespace Jira.Models.Entities
{
    [PrimaryKey(nameof(UserId), nameof(ProjectId))]
    public class ProjectMember
    {
        public required string UserId { get; set; }
        public required string ProjectId { get; set; }
        public required User User { get; set; }
        public required Project Project { get; set; }
        public required string Role { get; set; }
    }
}
