using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using VMart.Data;
using VMart.Models;
using VMart.Utility;

namespace VMart.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<IdentityUser> _userManager;
        public UserController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            this.db = db;
            _userManager = userManager;
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
            catch
            {
                TempData["error"] = "Failed to fetch users";
                return View(new List<UserViewModel>());
            }
        }

    }
}
