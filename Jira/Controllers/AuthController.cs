using Jira.Data;
using Jira.DTOs.Auth;
using Jira.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Jira.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(UserManager<User> userManager,
                                IConfiguration configuration,
                                SignInManager<User> signInManager,
                                AppDbContext appDbContext) : ControllerBase
    {
        private UserManager<User> UserManager { get; init; } = userManager;
        private SignInManager<User> SignInManager { get; init; } = signInManager;
        private IConfiguration? Configuration { get; init; } = configuration;
        private AppDbContext AppDbContext { get; init; } = appDbContext;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            User user = new()
            {
                UserName = dto.Login,
                Role = dto.Role
            };
            var result = await UserManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            await UserManager.AddClaimAsync(user, new Claim(ClaimTypes.Name, user.UserName));
            await UserManager.AddToRoleAsync(user, user.Role);
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await UserManager.FindByNameAsync(dto.Login);
            if (user == null) return Unauthorized();
            var result = await SignInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded) return Unauthorized();
            var userClaims = await UserManager.GetClaimsAsync(user);
            var claims = new List<Claim>
            {
                new(ClaimTypes.Role, user.Role),
                new(ClaimTypes.Sid, user.Id)
            }.Union(userClaims);
            var jwtSettings = Configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(3),
                signingCredentials: creds
            );

            return Ok(new
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expires = token.ValidTo
            });
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult WhoAmI() => Ok(AppDbContext.Users.Where(user => user.UserName == User.Identity.Name)
                                                              .Select(user => new
                                                              {
                                                                  user.Id,
                                                                  user.UserName,
                                                                  user.Role,
                                                              }));


        [HttpPatch("profile")]
        public async Task<IActionResult> UpdateMe([FromBody] UserUpdateDto dto)
        {
            await using var transaction = await AppDbContext.Database.BeginTransactionAsync();
            var me = await UserManager.FindByNameAsync(User.Identity.Name);
            if (dto.UserName != null)
            {
                var result = await UserManager.SetUserNameAsync(me, dto.UserName);
                if(!result.Succeeded)
                {
                    transaction.Rollback();
                    return BadRequest(result.Errors);
                }
                var claim = (await UserManager.GetClaimsAsync(me)).FirstOrDefault(claim => claim.Type == ClaimsIdentity.DefaultNameClaimType);
                await UserManager.ReplaceClaimAsync(me, claim, new Claim(ClaimsIdentity.DefaultNameClaimType, me.UserName));
            }
            if (dto.NewPassword != null)
            {
                var result = await UserManager.ChangePasswordAsync(me, dto.OldPassword, dto.NewPassword);
                if (!result.Succeeded)
                {
                    transaction.Rollback();
                    return BadRequest(result.Errors);
                }
            }
            if (dto.Role != null)
            {
                if (!await UserManager.IsInRoleAsync(me, "Admin"))
                {
                    transaction.Rollback();
                    return Forbid();
                }
                var roles = Configuration.GetSection("Roles").Get<List<string>>();
                if (!roles.Contains(dto.Role))
                {
                    transaction.Rollback();
                    return BadRequest(new { Error = "Некорректная роль" });
                }
                await UserManager.RemoveFromRoleAsync(me, me.Role);
                me.Role = dto.Role;
                await UserManager.AddToRoleAsync(me, dto.Role);
            }
            if (!ModelState.IsValid)
            {
                transaction.Rollback();
                return BadRequest(ModelState);
            }
            await UserManager.UpdateAsync(me);
            await transaction.CommitAsync();
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await UserManager.FindByIdAsync(id);
            if(user == null)
            {
                return NotFound();
            }
            await UserManager.DeleteAsync(user);
            return NoContent();
        }
    }
}
