using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using VMart.Data;
using VMart.Models;
using VMart.Utility;
using VMart.Interfaces;
using System;

namespace VMart.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class LogController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly ILogService logger;

        public LogController(ApplicationDbContext db, ILogService logger)
        {
            this.db = db;
            this.logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var logs = await db.Logs
                    .OrderByDescending(l => l.Timestamp)
                    .ToListAsync();

                return View(logs);
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Error loading log list", "Log", "Index", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Failed to load logs.";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var log = await db.Logs.FindAsync(id);
                if (log == null)
                {
                    await logger.LogAsync(SD.Log_Error, $"Log entry #{id} not found", "Log", "Details", null, Request.Path, User.Identity?.Name);
                    return NotFound();
                }

                return View(log);
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Error loading log details", "Log", "Details", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Failed to load log details.";
                return RedirectToAction("Index");
            }
        }
    }
}
