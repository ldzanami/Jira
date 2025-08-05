using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Jira.Models.Entities;

namespace Jira.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User>(options)
    {
        public DbSet<Column> Columns { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<Notification> Notifications {get; set;}
        public DbSet<Project> Projects {get; set;}
        public DbSet<ProjectMember> ProjectMembers {get; set;}
        public DbSet<TaskItem> TaskItems {get; set;}
        public DbSet<Tag> Tags {get; set;}

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Project>().HasMany(proj => proj.ProjectMembers)
                                     .WithOne(member => member.Project)
                                     .HasForeignKey(member => member.ProjectId)
                                     .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProjectMember>().HasOne(member => member.User)
                                           .WithMany(user => user.ProjectMemberships)
                                           .HasForeignKey(member => member.UserId);

            builder.Entity<Column>().HasMany(column => column.Tasks)
                                    .WithOne(task => task.Column)
                                    .HasForeignKey(task => task.ColumnId)
                                    .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TaskItem>().HasMany(task => task.Comments)
                                      .WithOne(comment => comment.TaskItem)
                                      .HasForeignKey(comment => comment.TaskItemId)
                                      .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TaskItem>().HasMany(task => task.Tags)
                                      .WithOne(tag => tag.TaskItem)
                                      .HasForeignKey(tag => tag.TaskItemId)
                                      .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
