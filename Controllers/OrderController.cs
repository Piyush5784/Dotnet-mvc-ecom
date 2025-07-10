using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VMart.Data;
using VMart.Models;
using VMart.Utility;
using VMart.Interfaces;
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
        private readonly ILogService logger;

        public OrderController(ApplicationDbContext db, UserManager<IdentityUser> userManager, ILogService logger)
        {
            this.db = db;
            this.userManager = userManager;
            this.logger = logger;
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
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Failed to load user orders", "Order", "Index", ex.ToString(), Request.Path, User.Identity?.Name);
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
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Failed to load order management", "Order", "Manage", ex.ToString(), Request.Path, User.Identity?.Name);
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
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, $"Failed to mark order #{id} as shipped", "Order", "MarkAsShipped", ex.ToString(), Request.Path, User.Identity?.Name);
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
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, $"Failed to cancel order #{id}", "Order", "CancelByUser", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Error cancelling the order.";
                return RedirectToAction("Index");
            }
        }
    }
}
