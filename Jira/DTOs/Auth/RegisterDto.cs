using System.ComponentModel.DataAnnotations;

namespace Jira.DTOs.Auth
{
    public class RegisterDto
    {
        public required string Login { get; set; }

        [DataType(DataType.Password)]
        public required string Password { get; set; }

        public required string Role { get; set; }
    }
}
