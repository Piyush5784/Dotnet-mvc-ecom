using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VMart.Data;
using VMart.Models;
using VMart.Utility;
using VMart.Interfaces;
using VMart.Services;
using VMart.Dto;
using System.Threading.Tasks;
using System;

namespace VMart.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly ILogService logger;
        private readonly ApiClientService apiClient;

        public ContactController(ApplicationDbContext db, ILogService logger, ApiClientService apiClient)
        {
            this.db = db;
            this.logger = logger;
            this.apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Try to get contact messages from API first
                var apiResponse = await apiClient.GetAsync<ApiResponseDto<List<Contact>>>("/api/Contact");

                if (apiResponse != null && apiResponse.Success && apiResponse.Data != null)
                {
                    return View(apiResponse.Data);
                }

                // Fallback to local database if API fails
                return View();
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
                // Try to get contact details from API first
                var apiResponse = await apiClient.GetAsync<ApiResponseDto<Contact>>($"/api/Contact/{id}");

                if (apiResponse != null && apiResponse.Success && apiResponse.Data != null)
                {
                    return View(apiResponse.Data);
                }

                return View();
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
