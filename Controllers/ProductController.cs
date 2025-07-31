using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMart.Data;
using VMart.Dto;
using VMart.Interfaces;
using VMart.Models;
using VMart.Services;
using VMart.Utility;

namespace VMart.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly ICloudinaryService cloud;
        private readonly ILogService logger;
        private readonly ApiClientService apiClient;

        public ProductController(ApiClientService apiClient, ApplicationDbContext db, ICloudinaryService cloud, ILogService logger)
        {
            this.db = db;
            this.cloud = cloud;
            this.apiClient = apiClient;
            this.logger = logger;
        }

        public async Task<IActionResult> Index(ProductFilterViewModel filter)
        {
            try
            {
                var result = await apiClient.GetAsync<ApiResponseDto<ProductFilterViewModel>>("/api/Product", filter);

                if (result?.Success == true && result.Data != null)
                {
                    return View(result.Data);
                }

                // Fallback to local database logic if API fails
                // var productsQuery = db.Product.Include(p => p.Category).AsQueryable();

                // if (!string.IsNullOrEmpty(filter.Search))
                //     productsQuery = productsQuery.Where(p => p.Title.Contains(filter.Search));

                // if (filter.MaxPrice.HasValue)
                //     productsQuery = productsQuery.Where(p => p.Price <= filter.MaxPrice.Value);

                // if (filter.MinRating.HasValue)
                //     productsQuery = productsQuery.Where(p => p.Ratings >= filter.MinRating.Value);

                // if (!string.IsNullOrEmpty(filter.Category))
                //     productsQuery = productsQuery.Where(p => p.Category != null && p.Category.Name == filter.Category);

                // switch (filter.SortBy)
                // {
                //     case "price-low":
                //         productsQuery = productsQuery.OrderBy(p => p.Price);
                //         break;
                //     case "price-high":
                //         productsQuery = productsQuery.OrderByDescending(p => p.Price);
                //         break;
                //     case "name":
                //         productsQuery = productsQuery.OrderBy(p => p.Title);
                //         break;
                //     default:
                //         productsQuery = productsQuery.OrderBy(p => p.Id);
                //         break;
                // }

                // int totalItems = productsQuery.Count();
                // filter.TotalPages = (int)Math.Ceiling(totalItems / (double)filter.PageSize);
                // filter.Products = productsQuery.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToList();
                // filter.Categories = db.Category.Select(c => c.Name).ToList();

                TempData["Warning"] = "Loaded products from local database (API unavailable).";
                return View(filter);
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Failed to load products", "Product", "Index", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Failed to load products.";
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> List(ProductFilterViewModel filter)
        {
            try
            {
                var result = await apiClient.GetAsync<ApiResponseDto<ProductFilterViewModel>>("/api/Product/list", filter);

                if (result?.Success == true && result.Data != null)
                {
                    return View(result.Data);
                }

                // Fallback to local database logic if API fails
                // var productsQuery = db.Product.Include(p => p.Category).AsQueryable();

                // if (!string.IsNullOrEmpty(filter.Search))
                //     productsQuery = productsQuery.Where(p => p.Title.Contains(filter.Search));

                // if (filter.MaxPrice.HasValue)
                //     productsQuery = productsQuery.Where(p => p.Price <= filter.MaxPrice.Value);

                // if (filter.MinRating.HasValue)
                //     productsQuery = productsQuery.Where(p => p.Ratings >= filter.MinRating.Value);

                // if (!string.IsNullOrEmpty(filter.Category))
                //     productsQuery = productsQuery.Where(p => p.Category != null && p.Category.Name == filter.Category);

                // switch (filter.SortBy)
                // {
                //     case "price-low": productsQuery = productsQuery.OrderBy(p => p.Price); break;
                //     case "price-high": productsQuery = productsQuery.OrderByDescending(p => p.Price); break;
                //     case "name": productsQuery = productsQuery.OrderBy(p => p.Title); break;
                //     default: productsQuery = productsQuery.OrderBy(p => p.Id); break;
                // }

                // filter.Products = await productsQuery.ToListAsync();
                // filter.Categories = await db.Category.Select(c => c.Name).ToListAsync();

                TempData["Warning"] = "Loaded products from local database (API unavailable).";
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
                if (id is null)
                {
                    return NotFound();
                }

                var result = await apiClient.GetAsync<ApiResponseDto<Product>>($"/api/Product/{id}");

                if (result?.Success == true && result.Data != null)
                {
                    return View(result.Data);
                }

                // Fallback to local database logic if API fails
                // var localProduct = await db.Product.FindAsync(id);
                // if (localProduct is null) return NotFound();

                TempData["Warning"] = "Product loaded from local database (API unavailable).";
                return View();
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Failed to load product details", "Product", "Details", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Failed to load product details.";
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Create()
        {
            var result = await apiClient.GetAsync<ApiResponseDto<List<Category>>>($"/api/Product/Categories");
            if (result?.Success == true && result.Data != null)
            {
                return View(new CreateProductDto { Categories = result.Data });
            }

            TempData["Warning"] = "Categories loaded from local database (API unavailable).";
            return View(new CreateProductDto { Categories = { } });
        }

        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public async Task<IActionResult> Create(CreateProductDto model)
        {
            // if (!ModelState.IsValid || model.Image is null)
            // {
            //     if (model.Image is null)
            //         ModelState.AddModelError("Image", "Please upload an image.");

            //     var categories = await apiClient.GetAsync<List<Category>>($"/api/Product/Categories");

            //     if (categories == null)
            //     {
            //         return View(model);
            //     }

            //     model.Categories = categories;
            //     return View(model);
            // }

            try
            {
                // var result = await cloud.UploadImageAsync(model.Image);

                // if (!result.Success)
                // {
                //     ModelState.AddModelError(string.Empty, result.ErrorMessage);
                //     model.Categories = GetOrderedCategories();
                //     return View(model);
                // }

                // var category = await db.Category.FindAsync(model.Product.CategoryId);
                // model.Product.ImageUrl = result.Url;
                // model.Product.Category = category;

                // db.Product.Add(model.Product);
                // await db.SaveChangesAsync();

                // TempData["Success"] = "Product created successfully!";
                var addProductResponse = await apiClient.PostAsync<ApiResponseDto<Product>>($"/api/Product", model);

                Console.WriteLine($"add product response : ${addProductResponse}");
                if (addProductResponse?.Success == true)
                {
                    TempData["Success"] = "Product created successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to create product via API.";
                }

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

                // Get product from API
                var productResult = await apiClient.GetAsync<ApiResponseDto<CreateProductDto>>($"/api/Product/edit/{id}");
                if (productResult?.Success == true && productResult.Data != null)
                {
                    return View(productResult.Data);
                }

                // Fallback to local processing if API fails
                TempData["Warning"] = "Product loaded from local database (API unavailable).";
                return NotFound();
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
            // if (!ModelState.IsValid)
            // {
            //     var categories = await apiClient.GetAsync<List<Category>>($"/api/Product/Categories");
            //     model.Categories = categories ?? new List<Category>();
            //     return View(model);
            // }

            try
            {
                // Send the update request to the API
                var result = await apiClient.PutAsync<ApiResponseDto<Product>>($"/api/Product/{model.Product.Id}", model);

                if (result?.Success == true)
                {
                    TempData["Success"] = "Product successfully updated!";
                    return RedirectToAction("List");
                }
                else
                {
                    TempData["Error"] = "Failed to update product via API.";
                    var categories = await apiClient.GetAsync<ApiResponseDto<List<Category>>>($"/api/Product/Categories");
                    model.Categories = categories?.Data ?? new List<Category>();
                    return View(model);
                }
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

                // Get product from API
                var productResult = await apiClient.GetAsync<ApiResponseDto<CreateProductDto>>($"/api/Product/delete/{id}");
                if (productResult?.Success == true && productResult.Data != null)
                {
                    return View(productResult.Data);
                }

                TempData["Warning"] = "Product loaded from local database (API unavailable).";
                return NotFound();
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

                // Send delete request to API
                var result = await apiClient.DeleteAsync<ApiResponseDto<object>>($"/api/Product/{id}");

                if (result?.Success == true)
                {
                    TempData["Success"] = "Product successfully deleted!";
                }
                else
                {
                    TempData["Error"] = "Failed to delete product via API.";
                }

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