using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMart.Data;
using VMart.Models;
using VMart.Utility;
using VMart.Interfaces;
using VMart.Models.ViewModels;
using VMart.Services;

namespace VMart.Controllers
{
    // [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogService logger;
        private JwtTokenService jwtTokenService;
        public UserController(ApplicationDbContext db, JwtTokenService jwtTokenService, UserManager<IdentityUser> userManager, ILogService logger)
        {
            this.jwtTokenService = jwtTokenService;
            this.db = db;
            this._userManager = userManager;
            this.logger = logger;
        }


        [HttpGet]
        public async Task<IActionResult> GetCurrentUserToken()
        {
            var jwtToken = "";
            if (HttpContext.User.Identity?.IsAuthenticated == true)
            {
                var existingToken = HttpContext.Session.GetString("token");
                var shouldGenerateNewToken = false;

                if (string.IsNullOrEmpty(existingToken))
                {
                    shouldGenerateNewToken = true;
                }
                else
                {
                    var principal = jwtTokenService.ValidateJwtToken(existingToken);
                    if (principal == null ||
                        !principal.FindAll(System.Security.Claims.ClaimTypes.Role)
                                  .Any(r => r.Value == SD.Role_Admin || r.Value == SD.Role_User))
                    {
                        shouldGenerateNewToken = true;
                    }
                }

                if (shouldGenerateNewToken)
                {
                    try
                    {
                        var user = await _userManager.GetUserAsync(HttpContext.User);
                        if (user != null)
                        {
                            jwtToken = await jwtTokenService.GenerateJwtTokenAsync(user);
                            HttpContext.Session.SetString("token", jwtToken);

                            var roles = await _userManager.GetRolesAsync(user);
                            var validRoles = roles.Where(r => r == SD.Role_Admin || r == SD.Role_User).ToList();

                            if (!validRoles.Any())
                            {
                                Console.WriteLine($"⚠️ User {user.UserName} has no valid roles");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Token generation failed: {ex.Message}");
                    }
                }
                else
                {
                    jwtToken = existingToken;
                }
            }

            return Ok(new { jwtToken });
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                List<ApplicationUser> users = await db.ApplicationUser.ToListAsync();
                var userWithRoles = new List<UserViewModel>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userWithRoles.Add(new UserViewModel
                    {
                        User = user,
                        Role = roles.FirstOrDefault() ?? "No role"
                    });
                }

                return View(userWithRoles);
            }
            catch (System.Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Failed to fetch users", "User", "Index", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["error"] = "Failed to fetch users";
                return View(new List<UserViewModel>());
            }
        }
    }
}