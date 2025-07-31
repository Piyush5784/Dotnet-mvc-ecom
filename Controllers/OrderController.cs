using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VMart.Data;
using VMart.Models;
using VMart.Utility;
using VMart.Interfaces;
using VMart.Services;
using System;
using System.Threading.Tasks;
using System.Linq;
using VMart.Dto;

namespace VMart.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<IdentityUser> userManager;
        private readonly ILogService logger;
        private readonly ApiClientService apiClient;

        public OrderController(ApplicationDbContext db, UserManager<IdentityUser> userManager, ILogService logger, ApiClientService apiClient)
        {
            this.db = db;
            this.userManager = userManager;
            this.logger = logger;
            this.apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Try to get user orders from API first
                var result = await apiClient.GetAsync<ApiResponseDto<List<Order>>>("/api/Order/user");

                // ...removed Console.WriteLine...
                if (result?.Success == true && result.Data != null)
                {
                    return View(result.Data);
                }
                // ...removed Console.WriteLine...

                // Fallback to local database if API fails
                var user = await userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var localOrders = await db.Order
                    .Include(o => o.Product)
                    .Where(o => o.ApplicationUserId == user.Id)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                TempData["Warning"] = "Loaded orders from local database (API unavailable).";
                return View(localOrders);
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
                // Try to get all orders from API first
                var result = await apiClient.GetAsync<ApiResponseDto<List<Order>>>("/api/Order/admin");

                if (result?.Success == true && result.Data != null)
                {
                    return View(result.Data);
                }


                TempData["Warning"] = "Loaded orders from local database (API unavailable).";
                return View();
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
                // Try to mark as shipped via API first
                var result = await apiClient.PostAsync<ApiResponseDto<object>>($"/api/Order/ship/{id}", new { });

                if (result?.Success == true)
                {
                    TempData["Success"] = $"Order #{id} marked as shipped.";
                    return RedirectToAction("Manage");
                }

                // Fallback to local processing if API fails
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
                // Try to cancel order via API first
                var result = await apiClient.PostAsync<ApiResponseDto<object>>($"/api/Order/cancel/{id}", new { });

                if (result?.Success == true)
                {
                    TempData["Success"] = $"Order #{id} has been cancelled.";
                    return RedirectToAction("Index");
                }

                // Fallback to local processing if API fails
                TempData["Warning"] = "Order cancelled locally (API unavailable).";
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
