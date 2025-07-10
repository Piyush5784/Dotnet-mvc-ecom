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
using VMart.Models;
using VMart.Utility;
using VMart.Interfaces;

namespace VMart.Controllers
{
    [Authorize(Roles = SD.Role_User)]
    public class CheckoutController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogService _logger;

        public CheckoutController(
            IConfiguration config,
            UserManager<IdentityUser> userManager,
            ApplicationDbContext db,
            ILogService logger)
        {
            _config = config;
            _userManager = userManager;
            _db = db;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var applicationUser = await _db.ApplicationUser.FindAsync(user.Id);
            if (applicationUser == null) return Unauthorized();

            var cartItems = await _db.Cart
                .Include(c => c.Product)
                .Where(c => c.ApplicationUserId == user.Id)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var vm = new CheckoutViewModel
            {
                StreetAddress = applicationUser.StreetAddress,
                City = applicationUser.City,
                State = applicationUser.State,
                PostalCode = applicationUser.PostalCode,
                CartItems = cartItems
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCheckoutSession(CheckoutViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var applicationUser = await _db.ApplicationUser.FindAsync(user.Id);
            if (applicationUser == null) return Unauthorized();

            var cartItems = await _db.Cart
                .Include(c => c.Product)
                .Where(c => c.ApplicationUserId == user.Id)
                .ToListAsync();

            model.CartItems = cartItems;

            if (!ModelState.IsValid)
            {
                await _logger.LogAsync(SD.Log_Error, "Invalid checkout address", "Checkout", "CreateCheckoutSession", null, Request.Path, user.UserName);
                TempData["Error"] = "Please fill out all address fields.";
                return View("Index", model);
            }

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            // Update user address
            applicationUser.StreetAddress = model.StreetAddress;
            applicationUser.City = model.City;
            applicationUser.State = model.State;
            applicationUser.PostalCode = model.PostalCode;

            await _db.SaveChangesAsync();

            // Validate stock
            foreach (var item in cartItems)
            {
                if (item.Quantity > item.Product.Quantity)
                {
                    await _logger.LogAsync(SD.Log_Warning, $"Stock issue on checkout: {item.Product.Title}", "Checkout", "CreateCheckoutSession", null, Request.Path, user.UserName);
                    TempData["Error"] = $"Only {item.Product.Quantity} left for {item.Product.Title}.";
                    return RedirectToAction("Index", "Cart");
                }
            }

            // Create Stripe session
            var domain = $"{Request.Scheme}://{Request.Host}";
            var lineItems = cartItems.Select(item => new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(item.Product.Price * 100),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.Product.Title
                    }
                },
                Quantity = item.Quantity
            }).ToList();

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = $"{domain}/Checkout/Success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{domain}/Checkout/Cancel?session_id={{CHECKOUT_SESSION_ID}}"
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Redirect(session.Url);
        }

        [HttpGet]
        public async Task<IActionResult> Success(string session_id)
        {
            if (string.IsNullOrEmpty(session_id)) return BadRequest();

            var stripeSession = new SessionService().Get(session_id);
            if (stripeSession.PaymentStatus != "paid")
            {
                await _logger.LogAsync(SD.Log_Error, $"Failed payment: {session_id}", "Checkout", "Success", null, Request.Path, User.Identity?.Name);
                TempData["Error"] = "Payment not completed.";
                return RedirectToAction("Index", "Cart");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var cartItems = await _db.Cart
                .Include(c => c.Product)
                .Where(c => c.ApplicationUserId == user.Id)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            // Create orders
            foreach (var item in cartItems)
            {
                var order = new Order
                {
                    ApplicationUserId = user.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price,
                    Status = SD.OrderStatusConfirmed,
                    paymentStatus = SD.Payment_Status_Completed,
                    PaymentSessionId = session_id,
                    OrderDate = DateTime.Now
                };
                _db.Order.Add(order);

                item.Product.Quantity -= item.Quantity;
            }

            _db.Cart.RemoveRange(cartItems);
            await _db.SaveChangesAsync();

            await _logger.LogAsync(SD.Log_Success, $"Order placed successfully for {user.UserName}", "Checkout", "Success", null, Request.Path, user.UserName);
            TempData["Success"] = "Payment successful! Your order is confirmed.";
            return View();
        }

        [HttpGet]
        public IActionResult Cancel(string session_id)
        {
            _logger.LogAsync(SD.Log_Info, $"Payment cancelled by user: {session_id}", "Checkout", "Cancel", null, Request.Path, User.Identity?.Name);
            TempData["Error"] = "Payment was cancelled.";
            return RedirectToAction("Index", "Cart");
        }
    }
}
