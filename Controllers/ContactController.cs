using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VMart.Data;
using VMart.Models;
using VMart.Utility;
using VMart.Interfaces;
using System.Threading.Tasks;
using System;

namespace VMart.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly ILogService logger;

        public ContactController(ApplicationDbContext db, ILogService logger)
        {
            this.db = db;
            this.logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var messages = await db.Contact
                    .OrderByDescending(c => c.Id)
                    .ToListAsync();

                return View(messages);
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Failed to load contact messages", "Contact", "Index", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Unable to display contact messages.";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var contact = await db.Contact.FindAsync(id);
                if (contact == null)
                {
                    await logger.LogAsync(SD.Log_Error, $"Contact entry #{id} not found", "Contact", "Details", null, Request.Path, User.Identity?.Name);
                    return NotFound();
                }

                return View(contact);
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Error loading contact details", "Contact", "Details", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Failed to load contact details.";
                return RedirectToAction("Index");
            }
        }
    }
}
