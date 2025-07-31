using Microsoft.AspNetCore.Mvc;
using VMart.Data;
using VMart.Models;
using VMart.Services;
using VMart.Interfaces;
using VMart.Utility;
using VMart.Dto;
using Microsoft.EntityFrameworkCore;
using VMart.Models.ViewModels;
using System;

namespace VMart.Controllers
{
    public class LatestProductController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly ApiClientService apiClient;
        private readonly ILogService logger;

        public LatestProductController(ApplicationDbContext db, ApiClientService apiClient, ILogService logger)
        {
            this.db = db;
            this.apiClient = apiClient;
            this.logger = logger;
        }


        public async Task<IActionResult> Index()
        {
            try
            {
                // Try to get latest products from API first using the generic DTO
                var apiResponse = await apiClient.GetAsync<ApiResponseDto<List<LatestProduct>>>("/api/LatestProduct");

                Console.WriteLine($"Latest product api response : {apiResponse}");

                if (apiResponse != null && apiResponse.Success && apiResponse.Data != null)
                {
                    TempData["Success"] = "Latest products loaded from API successfully.";
                    return View(apiResponse.Data);
                }

                Console.WriteLine($"API call unsuccessful or no data returned. Message: {apiResponse?.Message}");

                // Fallback to local database if API fails
                List<LatestProduct> localProducts = await db.LatestProduct
                       .Include(lp => lp.Product)
                       .OrderBy(lp => lp.DisplayOrder)
                       .ToListAsync();

                TempData["Warning"] = "Loaded latest products from local database (API unavailable).";
                return View(localProducts);
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Failed to load latest products", "LatestProduct", "Index", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Failed to load latest products.";
                return View(new List<LatestProduct>());
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                // Try to get create model from API first using the generic DTO
                var apiResponse = await apiClient.GetAsync<ApiResponseDto<AddLatestProductViewModel>>("/api/LatestProduct/create");

                if (apiResponse != null && apiResponse.Success && apiResponse.Data != null)
                {
                    TempData["Success"] = "Create form loaded from API successfully.";
                    return View(apiResponse.Data);
                }

                Console.WriteLine($"API call unsuccessful or no data returned. Message: {apiResponse?.Message}");

                // Fallback to local database if API fails
                var addModel = new AddLatestProductViewModel
                {
                    Products = await db.Product.ToListAsync(),
                    NewProduct = new LatestProduct()
                };

                TempData["Warning"] = "Create form loaded from local database (API unavailable).";
                return View(addModel);
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Failed to load create form", "LatestProduct", "Create", ex.ToString(), Request.Path, User.Identity?.Name);

                // Fallback to local data
                var addModel = new AddLatestProductViewModel
                {
                    Products = await db.Product.ToListAsync(),
                    NewProduct = new LatestProduct()
                };

                TempData["Error"] = "Failed to load create form from API, using local data.";
                return View(addModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddLatestProductViewModel data)
        {
            if (!ModelState.IsValid)
            {
                // Reload products for the view
                try
                {
                    var apiResponse = await apiClient.GetAsync<ApiResponseDto<AddLatestProductViewModel>>("/api/LatestProduct/create");
                    if (apiResponse?.Success == true && apiResponse.Data?.Products != null)
                    {
                        data.Products = apiResponse.Data.Products;
                    }
                    else
                    {
                        data.Products = await db.Product.ToListAsync();
                    }
                }
                catch
                {
                    data.Products = await db.Product.ToListAsync();
                }
                return View(data);
            }

            try
            {
                // Try to create via API first
                var apiResponse = await apiClient.PostAsync<ApiResponseDto<object>>("/api/LatestProduct/create", data);

                if (apiResponse != null && apiResponse.Success)
                {
                    TempData["Success"] = apiResponse.Message ?? "Latest product created via API successfully.";
                    return RedirectToAction("Index");
                }

                Console.WriteLine($"API call unsuccessful. Message: {apiResponse?.Message}");

                // Fallback to local processing if API fails
                var product = await db.Product.FindAsync(data.NewProduct.ProductId);

                if (product == null)
                {
                    ModelState.AddModelError("NewProduct.ProductId", "Invalid product selected.");
                    data.Products = await db.Product.ToListAsync();
                    return View(data);
                }

                data.NewProduct.Product = product;

                await db.LatestProduct.AddAsync(data.NewProduct);
                await db.SaveChangesAsync();

                await logger.LogAsync(SD.Log_Success, $"Latest product created locally for ProductId: {data.NewProduct.ProductId}", "LatestProduct", "Create", null, Request.Path, User.Identity?.Name);
                TempData["Success"] = "Latest product created successfully.";
                TempData["Warning"] = "Product created locally (API unavailable).";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Failed to create latest product", "LatestProduct", "Create", ex.ToString(), Request.Path, User.Identity?.Name);

                // Reload products and show error
                data.Products = await db.Product.ToListAsync();
                TempData["Error"] = "Failed to create latest product.";
                return View(data);
            }
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Try to get update model from API first
                var apiResponse = await apiClient.GetAsync<ApiResponseDto<AddLatestProductViewModel>>($"/api/LatestProduct/update/{id}");

                if (apiResponse != null && apiResponse.Success && apiResponse.Data != null)
                {
                    TempData["Success"] = "Update form loaded from API successfully.";
                    return View(apiResponse.Data);
                }

                Console.WriteLine($"API call unsuccessful or no data returned. Message: {apiResponse?.Message}");

                // Fallback to local database if API fails
                var product = await db.LatestProduct.FindAsync(id);

                if (product is null)
                {
                    return NotFound();
                }

                var AddModel = new AddLatestProductViewModel
                {
                    Products = await db.Product.ToListAsync(),
                    NewProduct = product
                };

                TempData["Warning"] = "Update form loaded from local database (API unavailable).";
                return View(AddModel);
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, $"Failed to load update form for ID: {id}", "LatestProduct", "Update", ex.ToString(), Request.Path, User.Identity?.Name);
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(AddLatestProductViewModel data)
        {
            if (!ModelState.IsValid)
            {
                // Reload products for the view
                try
                {
                    var apiResponse = await apiClient.GetAsync<ApiResponseDto<AddLatestProductViewModel>>($"/api/LatestProduct/update/{data.NewProduct.Id}");
                    if (apiResponse?.Success == true && apiResponse.Data?.Products != null)
                    {
                        data.Products = apiResponse.Data.Products;
                    }
                    else
                    {
                        data.Products = await db.Product.ToListAsync();
                    }
                }
                catch
                {
                    data.Products = await db.Product.ToListAsync();
                }
                return View(data);
            }

            try
            {
                // Try to update via API first
                var apiResponse = await apiClient.PutAsync<ApiResponseDto<object>>("/api/LatestProduct/update", data);

                if (apiResponse != null && apiResponse.Success)
                {
                    TempData["Success"] = apiResponse.Message ?? "Latest product updated via API successfully.";
                    return RedirectToAction("Index");
                }

                Console.WriteLine($"API call unsuccessful. Message: {apiResponse?.Message}");

                // Fallback to local processing if API fails
                var latestProductFromDb = await db.LatestProduct.FindAsync(data.NewProduct.Id);

                if (latestProductFromDb == null)
                {
                    return NotFound();
                }

                var selectedProduct = await db.Product.FindAsync(data.NewProduct.ProductId);

                if (selectedProduct == null)
                {
                    ModelState.AddModelError("NewProduct.ProductId", "Invalid product selected.");
                    data.Products = await db.Product.ToListAsync();
                    return View(data);
                }

                latestProductFromDb.ProductId = data.NewProduct.ProductId;
                latestProductFromDb.DisplayOrder = data.NewProduct.DisplayOrder;

                await db.SaveChangesAsync();

                await logger.LogAsync(SD.Log_Success, $"Latest product updated locally: ID {data.NewProduct.Id}", "LatestProduct", "Update", null, Request.Path, User.Identity?.Name);
                TempData["Success"] = "Latest product updated successfully.";
                TempData["Warning"] = "Product updated locally (API unavailable).";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Failed to update latest product", "LatestProduct", "Update", ex.ToString(), Request.Path, User.Identity?.Name);

                // Reload products and show error
                data.Products = await db.Product.ToListAsync();
                TempData["Error"] = "Failed to update latest product.";
                return View(data);
            }
        }



        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Try to get delete model from API first
                var apiResponse = await apiClient.GetAsync<ApiResponseDto<AddLatestProductViewModel>>($"/api/LatestProduct/delete/{id}");

                if (apiResponse != null && apiResponse.Success && apiResponse.Data != null)
                {
                    TempData["Success"] = "Delete form loaded from API successfully.";
                    return View(apiResponse.Data);
                }

                Console.WriteLine($"API call unsuccessful or no data returned. Message: {apiResponse?.Message}");

                // Fallback to local database if API fails
                var product = await db.LatestProduct.FindAsync(id);

                if (product is null)
                {
                    return NotFound();
                }

                var AddModel = new AddLatestProductViewModel
                {
                    Products = await db.Product.ToListAsync(),
                    NewProduct = product
                };

                TempData["Warning"] = "Delete form loaded from local database (API unavailable).";
                return View(AddModel);
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, $"Failed to load delete form for ID: {id}", "LatestProduct", "Delete", ex.ToString(), Request.Path, User.Identity?.Name);
                return NotFound();
            }
        }

        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteLatestProduct(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Try to delete via API first
                var apiResponse = await apiClient.DeleteAsync<ApiResponseDto<object>>($"/api/LatestProduct/delete/{id}");

                if (apiResponse != null && apiResponse.Success)
                {
                    TempData["Success"] = apiResponse.Message ?? "Latest product deleted via API successfully.";
                    return RedirectToAction("Index");
                }

                Console.WriteLine($"API call unsuccessful. Message: {apiResponse?.Message}");

                // Fallback to local processing if API fails
                var product = await db.LatestProduct.FindAsync(id);

                if (product is null)
                {
                    return NotFound();
                }

                db.LatestProduct.Remove(product);
                await db.SaveChangesAsync();

                await logger.LogAsync(SD.Log_Success, $"Latest product deleted locally: ID {id}", "LatestProduct", "Delete", null, Request.Path, User.Identity?.Name);
                TempData["Success"] = "Latest product deleted successfully.";
                TempData["Warning"] = "Product deleted locally (API unavailable).";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, $"Failed to delete latest product ID: {id}", "LatestProduct", "Delete", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Failed to delete latest product.";
                return RedirectToAction("Index");
            }
        }



    }
}
