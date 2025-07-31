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

                // Fallback to local database if API fails
                List<Feedback> localFeedbacks = await db.Feedback
                    .OrderByDescending(f => f.SubmittedAt)
                    .ToListAsync();

                TempData["Warning"] = "Loaded feedback from local database (API unavailable).";
                return View(localFeedbacks);
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

                // Fallback to local database if API fails
                var feedback = await db.Feedback.FindAsync(id);
                if (feedback == null)
                {
                    await logger.LogAsync(SD.Log_Error, $"Feedback entry #{id} not found", "Feedback", "Details", null, Request.Path, User.Identity?.Name);
                    TempData["Error"] = "Feedback not found.";
                    return RedirectToAction("Index");
                }

                TempData["Warning"] = "Loaded feedback details from local database (API unavailable).";
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
