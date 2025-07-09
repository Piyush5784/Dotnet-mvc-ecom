using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using VMart.Data;
using VMart.Models;
using VMart.Utility;
using System;
using System.Collections.Generic;

namespace VMart.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext db;

        public CategoryController(ApplicationDbContext db)
        {
            this.db = db;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                List<Category> categories = await db.Category.ToListAsync();
                return View(categories);
            }
            catch (Exception)
            {
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
                TempData["Success"] = "Category successfully created.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
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
            catch (Exception)
            {
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
                TempData["Success"] = "Category successfully updated.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
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
            catch (Exception)
            {
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

                TempData["Success"] = "Category successfully deleted.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["Error"] = "Error while deleting category.";
                return RedirectToAction("Index");
            }
        }
    }
}
