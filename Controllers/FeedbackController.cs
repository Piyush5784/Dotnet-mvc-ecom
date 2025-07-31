using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using VMart.Data;
using VMart.Models;
using VMart.Utility;
using VMart.Interfaces;
using VMart.Services;
using VMart.Dto;
using System;

namespace VMart.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly ILogService logger;
        private readonly ApiClientService apiClient;

        public FeedbackController(ApplicationDbContext db, ILogService logger, ApiClientService apiClient)
        {
            this.db = db;
            this.logger = logger;
            this.apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Try to get feedback from API first using the generic DTO
                var apiResponse = await apiClient.GetAsync<ApiResponseDto<List<Feedback>>>("/api/Feedback");

                if (apiResponse != null && apiResponse.Success && apiResponse.Data != null)
                {
                    TempData["Success"] = "Feedback loaded from API successfully.";
                    return View(apiResponse.Data);
                }

                Console.WriteLine($"API call unsuccessful or no data returned. Message: {apiResponse?.Message}");

                return View();
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
                // Try to get feedback details from API first using the generic DTO
                var apiResponse = await apiClient.GetAsync<ApiResponseDto<Feedback>>($"/api/Feedback/{id}");

                if (apiResponse != null && apiResponse.Success && apiResponse.Data != null)
                {
                    TempData["Success"] = "Feedback details loaded from API successfully.";
                    return View(apiResponse.Data);
                }

                Console.WriteLine($"API call unsuccessful or no data returned. Message: {apiResponse?.Message}");

                return View();
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
