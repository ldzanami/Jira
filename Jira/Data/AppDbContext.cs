using Jira.Infrastructure;
using Jira.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

            builder.Entity<Board>().HasMany(board => board.Columns)
                                   .WithOne(column => column.Board)
                                   .HasForeignKey(column => column.BoardId)
                                   .OnDelete(DeleteBehavior.Cascade);
        }

        public async Task<object> CheckForNull(string[]? userIds = null,
                                               string[]? memberIds = null,
                                               string? projectId = null,
                                               string[]? boardIds = null,
                                               string[]? columnIds = null,
                                               string[]? taskIds = null,
                                               string[]? commentIds = null)
        {
            if(userIds != null)
            {
                foreach (var userId in userIds)
                {
                    var user = await Users.Where(user => user.Id == userId).FirstOrDefaultAsync();
                    if (user == null)
                    {
                        return Constants.ErrorBuilder(Constants.UserNotFoundError, userId);
                    }
                }
            }

            if (memberIds != null && projectId != null)
            {
                foreach(var memberId in memberIds)
                {
                    var member = await ProjectMembers.Where(member => member.UserId == memberId && member.ProjectId == projectId).FirstOrDefaultAsync();
                    if (member == null)
                    {
                        return Constants.ErrorBuilder(Constants.ProjectMemberNotFoundError, memberId);
                    }
                }
            }

            if (projectId != null)
            {
                var project = await Projects.Where(proj => proj.Id == projectId).FirstOrDefaultAsync();
                if (project == null)
                {
                    return Constants.ErrorBuilder(Constants.ProjectNotFoundError, projectId);
                }
            }

            if(taskIds != null)
            {
                foreach (var taskId in taskIds)
                {
                    var task = await TaskItems.Where(task => task.Id == taskId).FirstOrDefaultAsync();
                    if (task == null)
                    {
                        return Constants.ErrorBuilder(Constants.TaskItemNotFoundError, taskId);
                    }
                }    
            }

            if (boardIds != null)
            {
                foreach (var boardId in boardIds)
                {
                    var board = await Boards.Where(board => board.Id == boardId).FirstOrDefaultAsync();
                    if (board == null)
                    {
                        return Constants.ErrorBuilder(Constants.BoardNotFoundError, boardId);
                    }
                }
            }

            if (columnIds != null)
            {
                foreach (var columnId in columnIds)
                {
                    var column = await Columns.Where(column => column.Id == columnId).FirstOrDefaultAsync();
                    if (column == null)
                    {
                        return Constants.ErrorBuilder(Constants.ColumnNotFoundError, columnId);
                    }
                }
            }

            if (commentIds != null)
            {
                foreach (var commentId in commentIds)
                {
                    var comment = await Comments.Where(comment => comment.Id == commentId).FirstOrDefaultAsync();
                    if (comment == null)
                    {
                        return Constants.ErrorBuilder(Constants.CommentNotFoundError, commentId);
                    }
                }
            }

            return null;
        }

        public async Task<bool> IsMember(string projectId,
                                         string userId,
                                         string[] requiredRoles) => await ProjectMembers.Where(member => member.UserId == userId && member.ProjectId == projectId)
                                                                                        .AnyAsync(member => requiredRoles.Contains(member.Role));

        public async Task<bool> IsRequiredOrAdmin(string projectId, ClaimsPrincipal User, string[] requiredRoles)
        {
            var isMember = await IsMember(projectId,
                                          User.Claims.SingleOrDefault(claim => claim.Type == ClaimTypes.Sid).Value,
                                          requiredRoles);

            var meUser = await Users.Where(user => user.UserName == User.Identity.Name).FirstOrDefaultAsync();

            return meUser.Role == "Admin" || isMember;
        }
    }
}