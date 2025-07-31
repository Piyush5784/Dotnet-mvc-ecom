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

                return View();
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

                return View();
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Failed to load create form", "LatestProduct", "Create", ex.ToString(), Request.Path, User.Identity?.Name);

                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddLatestProductViewModel data)
        {
            if (!ModelState.IsValid)
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

                data.Products = await db.Product.ToListAsync();

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
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await logger.LogAsync(SD.Log_Error, "Failed to create latest product", "LatestProduct", "Create", ex.ToString(), Request.Path, User.Identity?.Name);

                // Reload products and show error
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

                TempData["Warning"] = "Update form loaded from local database (API unavailable).";
                return Redirect("Index");
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

                return Redirect("Index");
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
