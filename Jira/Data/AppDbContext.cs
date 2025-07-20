using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Jira.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace Jira.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User>(options)
    {
        //public DbSet<Column> Columns {get; set;}
        //public DbSet<Comment> Comments {get; set;}
        //public DbSet<Board> Boards {get; set;}
        //public DbSet<Notification> Notifications {get; set;}
        //public DbSet<Project> Projects {get; set;}
        //public DbSet<ProjectMember> ProjectMembers {get; set;}
        //public DbSet<TaskItem> TaskItems {get; set;}
    }
}
