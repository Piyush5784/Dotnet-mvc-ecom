using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using VMart.Data;
using VMart.Models;
using VMart.Utility;
using VMart.Dto;
using System;
using System.Collections.Generic;
using VMart.Interfaces;
using VMart.Services;

namespace VMart.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly ILogService logger;
        private readonly ApiClientService apiClient;

        public CategoryController(ApplicationDbContext db, ILogService logger, ApiClientService apiClient)
        {
            this.db = db;
            this.logger = logger;
            this.apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var result = await apiClient.GetAsync<ApiResponseDto<List<Category>>>("/api/Category");

                if (result?.Success == true && result.Data != null)
                {
                    return View(result.Data);
                }

                TempData["Warning"] = "Failed to load categories from API.";
                return View(new List<Category>());
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Failed to load category list", "Category", "Index", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Failed to load categories.";
                return View(new List<Category>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            try
            {
                var result = await apiClient.PostAsync<ApiResponseDto<object>>("/api/Category", category);

                if (result?.Success == true)
                {
                    await logger.LogAsync(SD.Log_Success, $"Created category '{category.Name}'", "Category", "Create", null, Request.Path, User.Identity?.Name);
                    TempData["Success"] = "Category successfully created.";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "Error while creating category.";
                    return View(category);
                }
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Error while creating category", "Category", "Create", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Error while creating category.";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null) return NotFound();

                var result = await apiClient.GetAsync<ApiResponseDto<Category>>($"/api/Category/{id}");
                if (result?.Success == true && result.Data != null)
                {
                    return View(result.Data);
                }

                TempData["Warning"] = "Category loaded from local database (API unavailable).";
                return NotFound();
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Error loading category edit form", "Category", "Edit", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Error while loading edit form.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            try
            {
                var result = await apiClient.PutAsync<ApiResponseDto<object>>($"/api/Category/{category.Id}", category);

                if (result?.Success == true)
                {
                    await logger.LogAsync(SD.Log_Success, $"Updated category '{category.Name}'", "Category", "Edit", null, Request.Path, User.Identity?.Name);
                    TempData["Success"] = "Category successfully updated.";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "Error while updating category.";
                    return View(category);
                }
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Error updating category", "Category", "Edit", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Error while updating category.";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null || id == 0) return NotFound();

                var result = await apiClient.GetAsync<ApiResponseDto<Category>>($"/api/Category/{id}");
                if (result?.Success == true && result.Data != null)
                {
                    return View(result.Data);
                }

                TempData["Warning"] = "Category loaded from local database (API unavailable).";
                return NotFound();
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Error loading delete confirmation", "Category", "Delete", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Error while loading delete confirmation.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeletePOST(int? id)
        {
            try
            {
                if (id == null) return NotFound();

                var result = await apiClient.DeleteAsync<ApiResponseDto<object>>($"/api/Category/{id}");

                if (result?.Success == true)
                {
                    await logger.LogAsync(SD.Log_Success, $"Deleted category with ID {id}", "Category", "DeletePOST", null, Request.Path, User.Identity?.Name);
                    TempData["Success"] = "Category successfully deleted.";
                }
                else
                {
                    TempData["Error"] = "Error while deleting category.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Error deleting category", "Category", "DeletePOST", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Error while deleting category.";
                return RedirectToAction("Index");
            }
        }
    }
}
