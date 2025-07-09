using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VMart.Data;
using VMart.Models;
using VMart.Utility;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace VMart.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<IdentityUser> userManager;

        public OrderController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            this.db = db;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var orders = await db.Order
                    .Include(o => o.Product)
                    .Where(o => o.ApplicationUserId == user.Id)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                return View(orders);
            }
            catch (Exception)
            {
                TempData["Error"] = "Failed to load your orders.";
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Manage()
        {
            try
            {
                var orders = await db.Order
                    .Include(o => o.Product)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                return View(orders);
            }
            catch (Exception)
            {
                TempData["Error"] = "Failed to load order management.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> MarkAsShipped(int id)
        {
            try
            {
                var order = await db.Order.FindAsync(id);
                if (order == null)
                {
                    return NotFound();
                }

                order.Status = SD.OrderStatusShipped;
                await db.SaveChangesAsync();

                TempData["Success"] = $"Order #{order.Id} marked as shipped.";
                return RedirectToAction("Manage");
            }
            catch (Exception)
            {
                TempData["Error"] = "Error marking order as shipped.";
                return RedirectToAction("Manage");
            }
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_User)]
        public async Task<IActionResult> CancelByUser(int id)
        {
            try
            {
                var order = await db.Order
                    .Include(o => o.Product)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return NotFound();
                }

                if (order.Status == SD.OrderStatusCancelled)
                {
                    TempData["Error"] = $"Order #{order.Id} is already cancelled.";
                    return RedirectToAction("Index");
                }

                if (order.Product != null)
                {
                    order.Product.Quantity += order.Quantity;
                }

                order.Status = SD.OrderStatusCancelled;
                await db.SaveChangesAsync();

                TempData["Success"] = $"Order #{order.Id} has been cancelled.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["Error"] = "Error cancelling the order.";
                return RedirectToAction("Index");
            }
        }
    }
}
