using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Jira.Models.Entities
{
    public class User : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public required string Role { get; set; }
    }
}
