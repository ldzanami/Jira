using System.ComponentModel.DataAnnotations;

namespace Jira.DTOs.Auth
{
    public class LoginDto
    {
        public required string Login { get; set; }

        [DataType(DataType.Password)]
        public required string Password { get; set; }
    }
}
