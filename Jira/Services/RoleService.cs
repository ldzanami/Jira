using Microsoft.AspNetCore.Identity;

namespace Jira.Services
{
    public class RoleService(RoleManager<IdentityRole> roleManager)
    {
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        public async Task CreateRoleIfNotExists(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}
