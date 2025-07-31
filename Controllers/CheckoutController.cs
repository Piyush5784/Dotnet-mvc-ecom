using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;
using VMart.Data;
using VMart.Dto;
using VMart.Models;
using VMart.Utility;
using VMart.Interfaces;
using VMart.Models.ViewModels;
using VMart.Services;
using Newtonsoft.Json.Linq;

namespace VMart.Controllers
{
    [Authorize(Roles = SD.Role_User)]
    public class CheckoutController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogService _logger;
        private readonly ApiClientService _apiClient;

        public CheckoutController(
            IConfiguration config,
            UserManager<IdentityUser> userManager,
            ApplicationDbContext db,
            ILogService logger,
            ApiClientService apiClient)
        {
            _config = config;
            _userManager = userManager;
            _db = db;
            _logger = logger;
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Try to get checkout details from API first using the generic DTO
                var checkoutResponse = await _apiClient.GetAsync<ApiResponseDto<CheckoutViewModel>>("/api/Checkout");

                if (checkoutResponse?.Success == true)
                {
                    Console.WriteLine($"API Response - Success: {checkoutResponse.Success}, Message: {checkoutResponse.Message}");

                    Console.WriteLine($"Data : {checkoutResponse.Data}");

                    if (checkoutResponse.Success && checkoutResponse.Data != null)
                    {
                        // Ensure CartItems is not null before returning to view
                        if (checkoutResponse.Data.CartItems == null)
                        {
                            checkoutResponse.Data.CartItems = new List<Cart>();
                        }

                        Console.WriteLine($"Cart Items Count: {checkoutResponse.Data.CartItems.Count}");
                        TempData["Success"] = "Checkout loaded from API successfully.";
                        return View(checkoutResponse.Data);
                    }
                    else
                    {
                        Console.WriteLine($"API call unsuccessful or no data returned. Message: {checkoutResponse.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("No response received from API");
                }

                return View(new CheckoutViewModel());
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(SD.Log_Error, "Failed to load checkout", "Checkout", "Index", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Failed to load checkout page.";
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCheckoutSession(CheckoutViewModel model)
        {
            try
            {
                // Try to create checkout session via API first
                var apiResponse = await _apiClient.PostAsync<ApiResponseDto<object>>("/api/Checkout/session", model);

                if (apiResponse?.Success == true)
                {
                    Console.WriteLine($"API Response - Success: {apiResponse.Success}, Message: {apiResponse.Message}");

                    if (apiResponse.Data != null)
                    {
                        // Extract session URL from API response
                        var responseData = apiResponse.Data as JObject;
                        var sessionUrl = responseData?["sessionUrl"]?.ToString();

                        if (!string.IsNullOrEmpty(sessionUrl))
                        {
                            TempData["Success"] = "Checkout session created via API successfully.";
                            return Redirect(sessionUrl);
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"API call unsuccessful. Message: {apiResponse?.Message}");
                }

                // Fallback to local processing if API fails
                // var user = await _userManager.GetUserAsync(User);
                // if (user == null) return Unauthorized();

                // var applicationUser = await _db.ApplicationUser.FindAsync(user.Id);
                // if (applicationUser == null) return Unauthorized();

                // var cartItems = await _db.Cart
                //     .Include(c => c.Product)
                //     .Where(c => c.ApplicationUserId == user.Id)
                //     .ToListAsync();

                // model.CartItems = cartItems;

                // if (!ModelState.IsValid)
                // {
                //     await _logger.LogAsync(SD.Log_Error, "Invalid checkout address", "Checkout", "CreateCheckoutSession", null, Request.Path, user.UserName);
                //     TempData["Error"] = "Please fill out all address fields.";
                //     return View("Index", model);
                // }

                // if (cartItems.Count == 0)
                // {
                //     TempData["Error"] = "Your cart is empty.";
                //     return RedirectToAction("Index", "Cart");
                // }

                // // Update user address
                // applicationUser.StreetAddress = model.StreetAddress;
                // applicationUser.City = model.City;
                // applicationUser.State = model.State;
                // applicationUser.PostalCode = model.PostalCode;

                // await _db.SaveChangesAsync();

                // // Validate stock
                // foreach (var item in cartItems)
                // {
                //     if (item.Quantity > item.Product.Quantity)
                //     {
                //         await _logger.LogAsync(SD.Log_Warning, $"Stock issue on checkout: {item.Product.Title}", "Checkout", "CreateCheckoutSession", null, Request.Path, user.UserName);
                //         TempData["Error"] = $"Only {item.Product.Quantity} left for {item.Product.Title}.";
                //         return RedirectToAction("Index", "Cart");
                //     }
                // }

                // // Create Stripe session
                // var domain = $"{Request.Scheme}://{Request.Host}";
                // var lineItems = cartItems.Select(item => new SessionLineItemOptions
                // {
                //     PriceData = new SessionLineItemPriceDataOptions
                //     {
                //         UnitAmount = (long)(item.Product.Price * 100),
                //         Currency = "usd",
                //         ProductData = new SessionLineItemPriceDataProductDataOptions
                //         {
                //             Name = item.Product.Title
                //         }
                //     },
                //     Quantity = item.Quantity
                // }).ToList();

                // var options = new SessionCreateOptions
                // {
                //     PaymentMethodTypes = new List<string> { "card" },
                //     LineItems = lineItems,
                //     Mode = "payment",
                //     SuccessUrl = $"{domain}/Checkout/Success?session_id={{CHECKOUT_SESSION_ID}}",
                //     CancelUrl = $"{domain}/Checkout/Cancel?session_id={{CHECKOUT_SESSION_ID}}"
                // };

                // var service = new SessionService();
                // var session = service.Create(options);

                // TempData["Warning"] = "Checkout processed locally (API unavailable).";
                return Redirect("Index");
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(SD.Log_Error, "Error creating checkout session", "Checkout", "CreateCheckoutSession", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Error processing checkout.";
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Success(string session_id)
        {
            try
            {
                if (string.IsNullOrEmpty(session_id))
                {
                    TempData["Error"] = "Invalid session.";
                    return RedirectToAction("Index", "Cart");
                }

                // Try to process success via API first using the generic DTO
                var apiResponse = await _apiClient.GetAsync<ApiResponseDto<string>>($"/api/Checkout/success?session_id={session_id}");

                Console.WriteLine($"checkout-success : {apiResponse}, Success : {apiResponse?.Success}, data: {apiResponse?.Data}");

                if (apiResponse != null && apiResponse.Success)
                {
                    TempData["Success"] = apiResponse.Message ?? "Payment successful! Your order is confirmed.";
                    await _logger.LogAsync(SD.Log_Success, $"Order processed via API for session: {session_id}", "Checkout", "Success", null, Request.Path, User.Identity?.Name);
                    return View();
                }

                // Fallback to local processing if API fails
                // Console.WriteLine("API failed, falling back to local processing");

                // // Validate Stripe session first
                // var stripeSession = new SessionService().Get(session_id);
                // if (stripeSession == null || stripeSession.PaymentStatus != "paid")
                // {
                //     await _logger.LogAsync(SD.Log_Error, $"Failed payment verification: {session_id}, Status: {stripeSession?.PaymentStatus}", "Checkout", "Success", null, Request.Path, User.Identity?.Name);
                //     TempData["Error"] = "Payment not completed or session invalid.";
                //     return RedirectToAction("Index", "Cart");
                // }

                // var user = await _userManager.GetUserAsync(User);
                // if (user == null)
                // {
                //     TempData["Error"] = "User authentication failed.";
                //     return RedirectToAction("Login", "Account");
                // }

                // var cartItems = await _db.Cart
                //     .Include(c => c.Product)
                //     .Where(c => c.ApplicationUserId == user.Id)
                //     .ToListAsync();

                // // Check if order was already processed (to avoid duplicate processing)
                // var existingOrder = await _db.Order
                //     .FirstOrDefaultAsync(o => o.PaymentSessionId == session_id);

                // if (existingOrder != null)
                // {
                //     // Order already processed, just show success
                //     TempData["Success"] = "Payment successful! Your order is confirmed.";
                //     return View();
                // }

                // if (!cartItems.Any())
                // {
                //     // Cart might be empty if order was already processed by API
                //     // Check if there's an order with this session_id
                //     if (existingOrder == null)
                //     {
                //         TempData["Error"] = "No items found to process.";
                //         return RedirectToAction("Index", "Cart");
                //     }
                // }

                // // Validate stock before processing
                // foreach (var item in cartItems)
                // {
                //     if (item.Quantity > item.Product.Quantity)
                //     {
                //         await _logger.LogAsync(SD.Log_Error, $"Insufficient stock for {item.Product.Title}: requested {item.Quantity}, available {item.Product.Quantity}", "Checkout", "Success", null, Request.Path, user.UserName);
                //         TempData["Error"] = $"Insufficient stock for {item.Product.Title}. Please contact support.";
                //         return RedirectToAction("Index", "Cart");
                //     }
                // }

                // // Create orders
                // foreach (var item in cartItems)
                // {
                //     var order = new Order
                //     {
                //         ApplicationUserId = user.Id,
                //         ProductId = item.ProductId,
                //         Quantity = item.Quantity,
                //         Price = item.Product.Price,
                //         Status = SD.OrderStatusConfirmed,
                //         paymentStatus = SD.Payment_Status_Completed,
                //         PaymentSessionId = session_id,
                //         OrderDate = DateTime.Now
                //     };
                //     _db.Order.Add(order);

                //     // Update product quantity
                //     item.Product.Quantity -= item.Quantity;
                //     _db.Product.Update(item.Product);
                // }

                // // Clear cart
                // _db.Cart.RemoveRange(cartItems);
                // await _db.SaveChangesAsync();

                // await _logger.LogAsync(SD.Log_Success, $"Order placed successfully for {user.UserName} via local processing", "Checkout", "Success", null, Request.Path, user.UserName);
                // TempData["Success"] = "Payment successful! Your order is confirmed.";
                // TempData["Warning"] = "Order processed locally (API unavailable).";
                return View();
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(SD.Log_Error, "Error processing payment success", "Checkout", "Success", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Error processing your order. Please contact support.";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Cancel(string session_id)
        {
            try
            {
                // Try to process cancellation via API first using the generic DTO
                // Note: API might not have a cancel endpoint, so this is commented out
                var apiResponse = await _apiClient.GetAsync<ApiResponseDto<string>>($"/api/Checkout/cancel?session_id={session_id}");

                if (apiResponse != null && apiResponse.Success)
                {
                    TempData["Info"] = "Payment was cancelled.";
                    return View();
                }

                // Log the cancellation
                await _logger.LogAsync(SD.Log_Info, $"Payment cancelled by user: {session_id}", "Checkout", "Cancel", null, Request.Path, User.Identity?.Name);
                TempData["Info"] = "Payment was cancelled. Your cart items are still saved.";
                TempData["Warning"] = "Cancellation processed locally (API unavailable).";
                return View();
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(SD.Log_Error, "Error processing payment cancellation", "Checkout", "Cancel", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Payment was cancelled.";
                return View();
            }
        }
    }
}
