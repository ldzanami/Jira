using Jira.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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
        public required List<ProjectMember> ProjectMembers { get; set; } = [];
        public required List<Board> Boards { get; set; } = [];

        public async Task<List<ProjectMember>> GetProjectMembersAsync(AppDbContext dbContext) => await dbContext.ProjectMembers.Where(member => member.ProjectId == Id).ToListAsync();
        public async Task<List<Board>> GetBoardsAsync(AppDbContext dbContext) => await dbContext.Boards.Where(board => board.ProjectId == Id).ToListAsync();
    }
}