using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using VMart.Data;
using VMart.Models;
using VMart.Utility;
using System;
using VMart.Interfaces;
using VMart.Services;
using VMart.Dto;

namespace VMart.Controllers
{
    [Authorize(Roles = SD.Role_User)]
    public class CartController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly ApplicationDbContext db;
        private readonly ILogService logEntry;
        private readonly ApiClientService apiClient;

        public CartController(ApiClientService apiClient, UserManager<IdentityUser> userManager, ApplicationDbContext db, ILogService logEntry)
        {
            this.logEntry = logEntry;
            this.apiClient = apiClient;
            this.userManager = userManager;
            this.db = db;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var apiResponse = await apiClient.GetAsync<ApiResponseDto<List<CartDto>>>("/api/Cart");

                if (apiResponse?.Success == true && apiResponse.Data != null)
                {
                    return View(apiResponse.Data);
                }

                // Fallback to local database if API is unavailable or returns error
                var user = await userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["Error"] = "Please log in to view your cart.";
                    return View(new List<CartDto>());
                }

                var localCartItems = await db.Cart
                    .Include(c => c.Product)
                    .Where(c => c.ApplicationUserId == user.Id)
                    .ToListAsync();

                var cartDtos = localCartItems.Select(c =>
                {
                    var userId = Guid.TryParse(c.ApplicationUserId, out var parsedId) ? parsedId : Guid.Empty;
                    return new CartDto
                    {
                        Id = c.Id,
                        UserId = userId,
                        ProductId = c.ProductId,
                        ProductName = c.Product != null ? c.Product.Title : "Unknown Product",
                        ProductDescription = c.Product != null ? c.Product.Description : "",
                        ProductPrice = c.Product != null ? c.Product.Price : 0,
                        ProductImageUrl = c.Product != null ? c.Product.ImageUrl : "",
                        Quantity = c.Quantity
                    };
                }).ToList();

                if (cartDtos.Any())
                {
                    TempData["error"] = "Loaded cart from local database (API unavailable).";
                }
                else if (apiResponse?.Success == false)
                {
                    TempData["Error"] = apiResponse.Message ?? "Failed to load cart from API.";
                }

                return View(cartDtos);
            }
            catch (Exception ex)
            {
                await logEntry.LogAsync(SD.Log_Error, "Failed to load cart", "Cart", "Index", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Failed to load your cart.";
                return View(new List<CartDto>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddToCart(int? id)
        {
            try
            {
                if (id == null) return BadRequest();

                var apiResponse = await apiClient.PostAsync<ApiResponseDto<object>>($"/api/Cart/add/{id}", new { });

                Console.WriteLine($"Response : {apiResponse}");

                if (apiResponse?.Success == true)
                {
                    TempData["Success"] = apiResponse.Message ?? "Item added to cart!";
                }
                else
                {
                    TempData["Error"] = apiResponse?.Message ?? "Error adding item to cart.";
                }

                return RedirectToAction("Index", "Cart");
            }
            catch (Exception ex)
            {
                await logEntry.LogAsync(SD.Log_Error, "Error adding item to cart", "Cart", "AddToCart", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Error adding item to cart.";
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Remove(int? id)
        {
            try
            {
                if (id == null) return BadRequest();

                var apiResponse = await apiClient.DeleteAsync<ApiResponseDto<object>>($"/api/Cart/remove/{id}");

                if (apiResponse?.Success == true)
                {
                    TempData["Success"] = apiResponse.Message ?? "Item removed from cart.";
                }
                else
                {
                    TempData["Error"] = apiResponse?.Message ?? "Error removing item from cart.";
                }

                return RedirectToAction("Index", "Cart");
            }
            catch (Exception ex)
            {
                await logEntry.LogAsync(SD.Log_Error, "Error Removing item from cart", "Cart", "Remove", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Error removing item from cart.";
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Decrease(int? id)
        {
            try
            {
                if (id == null) return BadRequest();

                var apiResponse = await apiClient.PostAsync<ApiResponseDto<object>>($"/api/Cart/decrease/{id}", new { });

                if (apiResponse?.Success == true)
                {
                    TempData["Success"] = apiResponse.Message ?? "Item quantity decreased.";
                }
                else
                {
                    TempData["Error"] = apiResponse?.Message ?? "Error decreasing item quantity.";
                }

                return RedirectToAction("Index", "Cart");
            }
            catch (Exception ex)
            {
                await logEntry.LogAsync(SD.Log_Error, "Error decreasing item from cart", "Cart", "Decrease", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Error removing item quantity.";
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Increase(int? id)
        {
            try
            {
                if (id == null) return BadRequest();

                var apiResponse = await apiClient.PostAsync<ApiResponseDto<object>>($"/api/Cart/increase/{id}", new { });

                if (apiResponse?.Success == true)
                {
                    TempData["Success"] = apiResponse.Message ?? "Item quantity increased.";
                }
                else
                {
                    TempData["Error"] = apiResponse?.Message ?? "Error increasing item quantity.";
                }

                return RedirectToAction("Index", "Cart");
            }
            catch (Exception ex)
            {
                await logEntry.LogAsync(SD.Log_Error, "Error increasing item quantity", "Cart", "Increase", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Error increasing item quantity.";
                return RedirectToAction("Index", "Cart");
            }
        }
    }
}
