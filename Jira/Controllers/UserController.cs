
using Jira.Data;
using Jira.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Jira.Controllers
{
    public class UserController(AppDbContext appDbContext, SignInManager<User> signInManager) : ControllerBase
    {
        private AppDbContext AppDbContext { get; set; } = appDbContext;
        private SignInManager<User> SignInManager { get; set; } = signInManager;
    }
}
