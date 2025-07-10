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

namespace VMart.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogService logger;

        public UserController(ApplicationDbContext db, UserManager<IdentityUser> userManager, ILogService logger)
        {
            this.db = db;
            _userManager = userManager;
            this.logger = logger;
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