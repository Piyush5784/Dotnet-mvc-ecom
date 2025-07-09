using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;
using VMart.Data;
using VMart.Models;
using VMart.Utility;
using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace VMart.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext db;
        private readonly EmailSender emailSender;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db, EmailSender emailSender)
        {
            _logger = logger;
            this.db = db;
            this.emailSender = emailSender;
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
                _logger.LogError(ex, "Error loading homepage products");
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
                _logger.LogError(ex, "Error submitting feedback");
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
                _logger.LogError(ex, "Error sending contact message");
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
