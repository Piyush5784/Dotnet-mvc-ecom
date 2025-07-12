using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VMart.Data;
using VMart.Dto;
using VMart.Models;
using VMart.Utility;
using VMart.Interfaces;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace VMart.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly ICloudinaryService cloud;
        private readonly ILogService logger;

        public ProductController(ApplicationDbContext db, ICloudinaryService cloud, ILogService logger)
        {
            this.db = db;
            this.cloud = cloud;
            this.logger = logger;
        }

        public IActionResult Index(ProductFilterViewModel filter)
        {
            try
            {
                var productsQuery = db.Product.Include(p => p.Category).AsQueryable();

                if (!string.IsNullOrEmpty(filter.Search))
                    productsQuery = productsQuery.Where(p => p.Title.Contains(filter.Search));

                if (filter.MaxPrice.HasValue)
                    productsQuery = productsQuery.Where(p => p.Price <= filter.MaxPrice.Value);

                if (filter.MinRating.HasValue)
                    productsQuery = productsQuery.Where(p => p.Ratings >= filter.MinRating.Value);

                if (!string.IsNullOrEmpty(filter.Category))
                    productsQuery = productsQuery.Where(p => p.Category.Name == filter.Category);

                switch (filter.SortBy)
                {
                    case "price-low":
                        productsQuery = productsQuery.OrderBy(p => p.Price);
                        break;
                    case "price-high":
                        productsQuery = productsQuery.OrderByDescending(p => p.Price);
                        break;
                    case "name":
                        productsQuery = productsQuery.OrderBy(p => p.Title);
                        break;
                    default:
                        productsQuery = productsQuery.OrderBy(p => p.Id);
                        break;
                }

                int totalItems = productsQuery.Count();
                filter.TotalPages = (int)Math.Ceiling(totalItems / (double)filter.PageSize);
                filter.Products = productsQuery.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToList();
                filter.Categories = db.Category.Select(c => c.Name).ToList();

                return View(filter);
            }
            catch (Exception ex)
            {
                logger.LogAsync(SD.Log_Error, "Failed to load products", "Product", "Index", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Failed to load products.";
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> List(ProductFilterViewModel filter)
        {
            try
            {
                var productsQuery = db.Product.Include(p => p.Category).AsQueryable();

                if (!string.IsNullOrEmpty(filter.Search))
                    productsQuery = productsQuery.Where(p => p.Title.Contains(filter.Search));

                if (filter.MaxPrice.HasValue)
                    productsQuery = productsQuery.Where(p => p.Price <= filter.MaxPrice.Value);

                if (filter.MinRating.HasValue)
                    productsQuery = productsQuery.Where(p => p.Ratings >= filter.MinRating.Value);

                if (!string.IsNullOrEmpty(filter.Category))
                    productsQuery = productsQuery.Where(p => p.Category.Name == filter.Category);

                switch (filter.SortBy)
                {
                    case "price-low": productsQuery = productsQuery.OrderBy(p => p.Price); break;
                    case "price-high": productsQuery = productsQuery.OrderByDescending(p => p.Price); break;
                    case "name": productsQuery = productsQuery.OrderBy(p => p.Title); break;
                    default: productsQuery = productsQuery.OrderBy(p => p.Id); break;
                }

                filter.Products = await productsQuery.ToListAsync();
                filter.Categories = await db.Category.Select(c => c.Name).ToListAsync();

                return View(filter);
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Unable to load filtered product list", "Product", "List", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Unable to load filtered product list.";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                var product = await db.Product.FindAsync(id);
                if (product is null) return NotFound();
                return View(product);
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Failed to load product details", "Product", "Details", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Failed to load product details.";
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult Create()
        {
            return View(new CreateProductDto { Categories = GetOrderedCategories() });
        }

        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public async Task<IActionResult> Create(CreateProductDto model)
        {
            if (!ModelState.IsValid || model.Image is null)
            {
                if (model.Image is null)
                    ModelState.AddModelError("Image", "Please upload an image.");

                model.Categories = GetOrderedCategories();
                return View(model);
            }

            try
            {
                var result = await cloud.UploadImageAsync(model.Image);

                if (!result.Success)
                {
                    ModelState.AddModelError(string.Empty, result.ErrorMessage);
                    model.Categories = GetOrderedCategories();
                    return View(model);
                }

                var category = await db.Category.FindAsync(model.Product.CategoryId);
                model.Product.ImageUrl = result.Url;
                model.Product.Category = category;

                db.Product.Add(model.Product);
                await db.SaveChangesAsync();

                TempData["Success"] = "Product created successfully!";
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Error while creating product", "Product", "Create", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "An error occurred while creating the product.";
                return RedirectToAction("List");
            }
        }

        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id is null) return NotFound();

                var product = await db.Product.FindAsync(id);
                if (product is null) return NotFound();

                return View(new CreateProductDto
                {
                    Product = product,
                    Categories = GetOrderedCategories()
                });
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Failed to load edit form", "Product", "Edit", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Failed to load edit form.";
                return RedirectToAction("List");
            }
        }

        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public async Task<IActionResult> Edit(CreateProductDto model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = GetOrderedCategories();
                return View(model);
            }

            try
            {
                if (model.Image != null && model.Image.Length > 0)
                {
                    var result = await cloud.UploadImageAsync(model.Image);
                    if (!result.Success)
                    {
                        ModelState.AddModelError(string.Empty, result.ErrorMessage);
                        model.Categories = GetOrderedCategories();
                        return View(model);
                    }

                    var category = await db.Category.FindAsync(model.Product.CategoryId);
                    model.Product.ImageUrl = result.Url;
                    model.Product.Category = category;
                }
                else
                {
                    var existing = await db.Product.AsNoTracking().FirstOrDefaultAsync(p => p.Id == model.Product.Id);
                    if (existing != null)
                    {
                        model.Product.ImageUrl = existing.ImageUrl;
                    }
                }

                db.Product.Update(model.Product);
                await db.SaveChangesAsync();

                TempData["Success"] = "Product successfully updated!";
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Failed to update product", "Product", "Edit", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Failed to update product.";
                return RedirectToAction("List");
            }
        }

        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id is null) return NotFound();

                var product = await db.Product.FindAsync(id);
                if (product is null) return NotFound();

                return View(new CreateProductDto
                {
                    Product = product,
                    Categories = GetOrderedCategories()
                });
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Failed to load delete form", "Product", "Delete", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Failed to load delete confirmation.";
                return RedirectToAction("List");
            }
        }

        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeletePOST(int? id)
        {
            try
            {
                if (id is null) return NotFound();

                var product = await db.Product.FindAsync(id);
                if (product is null) return NotFound();

                db.Product.Remove(product);
                await db.SaveChangesAsync();

                TempData["Success"] = "Product successfully deleted!";
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Error while deleting product", "Product", "DeletePOST", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Error while deleting product.";
                return RedirectToAction("List");
            }
        }

        private List<Category> GetOrderedCategories()
        {
            try
            {
                return db.Category.OrderBy(c => c.DisplayOrder).ToList();
            }
            catch (Exception ex)
            {
                logger.LogAsync(SD.Log_Error, "Error loading categories", "Product", "GetOrderedCategories", ex.ToString(), Request.Path, User.Identity?.Name);
                return new List<Category>();
            }
        }
    }
}