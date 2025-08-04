using System.ComponentModel.DataAnnotations;

namespace Jira.DTOs.Auth
{
    public class UserUpdateDto
    {
        public string? UserName { get; set; }

        [DataType(DataType.Password)]
        public string? OldPassword { get; set; }

        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        public string? Role { get; set; }
    }
}
