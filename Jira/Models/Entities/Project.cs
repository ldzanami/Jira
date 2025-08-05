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
        public List<ProjectMember> ProjectMembers { get; set; } = [];
        public List<Board> Boards { get; set; } = [];
    }
}