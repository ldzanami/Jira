using Jira.Data;
using Microsoft.EntityFrameworkCore;

namespace Jira.Models.Entities
{
    public class Project
    {
        public required string ProjectId { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string ProjectMasterId { get; set; }
        public User? ProjectMaster { get; set; }

        public async Task<List<ProjectMember>> GetProjectMembers(AppDbContext dbContext) => await dbContext.ProjectMembers.Where(member => member.ProjectId == ProjectId).ToListAsync();
    }
}