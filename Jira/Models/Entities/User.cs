using Microsoft.AspNetCore.Identity;

namespace Jira.Models.Entities
{
    public class User : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public required string Role { get; set; }
    }
}
