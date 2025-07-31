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
    public class LogController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly ILogService logger;
        private readonly ApiClientService apiClient;

        public LogController(ApplicationDbContext db, ILogService logger, ApiClientService apiClient)
        {
            this.db = db;
            this.logger = logger;
            this.apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Try to get logs from API first using the generic DTO
                var apiResponse = await apiClient.GetAsync<ApiResponseDto<List<Logs>>>("/api/Log");

                if (apiResponse != null && apiResponse.Success && apiResponse.Data != null)
                {
                    TempData["Success"] = "Logs loaded from API successfully.";
                    return View(apiResponse.Data);
                }

                Console.WriteLine($"API call unsuccessful or no data returned. Message: {apiResponse?.Message}");

                TempData["Warning"] = "Loaded logs from local database (API unavailable).";
                return View();
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
                // Try to get log details from API first using the generic DTO
                var apiResponse = await apiClient.GetAsync<ApiResponseDto<Logs>>($"/api/Log/{id}");

                if (apiResponse != null && apiResponse.Success && apiResponse.Data != null)
                {
                    TempData["Success"] = "Log details loaded from API successfully.";
                    return View(apiResponse.Data);
                }

                Console.WriteLine($"API call unsuccessful or no data returned. Message: {apiResponse?.Message}");

                return View();
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
