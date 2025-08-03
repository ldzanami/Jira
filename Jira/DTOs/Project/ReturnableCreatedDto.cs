
namespace Jira.DTOs.Project
{
    public class ReturnableCreatedDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string OwnerId { get; set; }
        public required string OwnerName { get; set; }
        public required DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public static explicit operator ReturnableCreatedDto(Jira.Models.Entities.Project project) => new() { Id = project.Id,
                                                                                                              Name = project.Name,
                                                                                                              Description = project.Description,
                                                                                                              OwnerId = project.OwnerId,
                                                                                                              OwnerName = project.Owner.UserName,
                                                                                                              CreatedAt = project.CreatedAt,
                                                                                                              UpdatedAt = project.UpdatedAt
                                                                                                             };
    }
}