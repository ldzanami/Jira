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
    }
}