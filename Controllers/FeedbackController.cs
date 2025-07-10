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
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly ILogService logger;

        public FeedbackController(ApplicationDbContext db, ILogService logger)
        {
            this.db = db;
            this.logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                List<Feedback> feedbacks = await db.Feedback
                    .OrderByDescending(f => f.SubmittedAt)
                    .ToListAsync();

                return View(feedbacks);
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Failed to load feedback list", "Feedback", "Index", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Unable to display feedback.";
                return RedirectToAction("Index", "Home");
            }
        }


        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var feedback = await db.Feedback.FindAsync(id);
                if (feedback == null)
                {
                    await logger.LogAsync(SD.Log_Error, $"Feedback entry #{id} not found", "Feedback", "Details", null, Request.Path, User.Identity?.Name);
                    return NotFound();
                }

                return View(feedback);
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Error loading feedback details", "Feedback", "Details", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Failed to load feedback details.";
                return RedirectToAction("Index");
            }
        }

    }
}
