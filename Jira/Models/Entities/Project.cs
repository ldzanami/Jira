using Jira.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Jira.Models.Entities
{
    public class Project
    {
        [Key]
        public required string Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string OwnerId { get; set; }
        public User? Owner { get; set; }
        public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public List<ProjectMember> GetProjectMembers(AppDbContext dbContext) => dbContext.ProjectMembers.Where(member => member.ProjectId == Id)
                                                                                                        .Include(member => member.Project)
                                                                                                        .Include(member => member.User)
                                                                                                        .ToList();

        public List<Board> GetBoards(AppDbContext dbContext) => dbContext.Boards.Where(board => board.ProjectId == Id)
                                                                                .Include(board => board.Project)
                                                                                .ToList();

        public bool IsMember(ClaimsPrincipal user, AppDbContext appDbContext)
        {
            var members = GetProjectMembers(appDbContext);
            return members.Any(member => member.User.UserName == user.Identity.Name);
        }
    }
}