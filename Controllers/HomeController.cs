using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VMart.Data;
using VMart.Models;
using VMart.Utility;
using VMart.Interfaces;
using VMart.Services;

namespace VMart.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IEmailSenderApplicationInterface emailSender;
        private readonly ILogService logger;

        public HomeController(ApplicationDbContext db, IEmailSenderApplicationInterface emailSender, ILogService logger)
        {
            this.db = db;
            this.emailSender = emailSender;
            this.logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                List<Product> products = await db.Product.ToListAsync();
                return View(products);
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Error loading homepage products", "Home", "Index", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Failed to load products.";
                return RedirectToAction("Error");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [Authorize(Roles = SD.Role_User)]
        public IActionResult Feedback()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_User)]
        public async Task<IActionResult> Feedback(Feedback feedback)
        {
            if (!ModelState.IsValid)
            {
                return View(feedback);
            }

            try
            {
                db.Feedback.Add(feedback);
                await db.SaveChangesAsync();

                TempData["Success"] = "Thank you for your feedback!";
                return RedirectToAction("Feedback");
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Failed to submit feedback", "Home", "Feedback", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Failed to submit feedback.";
                return RedirectToAction("Feedback");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Contact(Contact c)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            try
            {
                db.Contact.Add(c);
                await db.SaveChangesAsync();
                await emailSender.SendContactMessageToAdminAsync(c);

                TempData["Success"] = "Message successfully sent!";
                return RedirectToAction("Contact");
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Failed to send contact message", "Home", "Contact", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Something went wrong while sending your message.";
                return RedirectToAction("Contact");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
