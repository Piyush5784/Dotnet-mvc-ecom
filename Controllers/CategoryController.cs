using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using VMart.Data;
using VMart.Models;
using VMart.Utility;
using System;
using System.Collections.Generic;
using VMart.Interfaces;

namespace VMart.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly ILogService logger;

        public CategoryController(ApplicationDbContext db, ILogService logger)
        {
            this.db = db;
            this.logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                List<Category> categories = await db.Category.ToListAsync();
                return View(categories);
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Failed to load category list", "Category", "Index", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Failed to load categories.";
                return RedirectToAction("Index");
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
                db.Category.Add(category);
                await db.SaveChangesAsync();

                await logger.LogAsync(SD.Log_Success, $"Created category '{category.Name}'", "Category", "Create", null, Request.Path, User.Identity?.Name);
                TempData["Success"] = "Category successfully created.";
                return RedirectToAction("Index");
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

                var category = await db.Category.FindAsync(id);
                if (category == null) return NotFound();

                return View(category);
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
                db.Category.Update(category);
                await db.SaveChangesAsync();

                await logger.LogAsync(SD.Log_Success, $"Updated category '{category.Name}'", "Category", "Edit", null, Request.Path, User.Identity?.Name);
                TempData["Success"] = "Category successfully updated.";
                return RedirectToAction("Index");
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

                var category = await db.Category.FindAsync(id);
                if (category == null) return NotFound();

                return View(category);
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
                var category = await db.Category.FindAsync(id);
                if (category == null) return NotFound();

                db.Category.Remove(category);
                await db.SaveChangesAsync();

                await logger.LogAsync(SD.Log_Success, $"Deleted category '{category.Name}'", "Category", "DeletePOST", null, Request.Path, User.Identity?.Name);
                TempData["Success"] = "Category successfully deleted.";
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
