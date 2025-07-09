using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;
using VMart.Data;
using VMart.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMart.Utility;

namespace VMart.Controllers
{
    [Authorize(Roles = SD.Role_User)]
    public class CheckoutController : Controller
    {
        private readonly IConfiguration config;
        private readonly ApplicationDbContext db;
        private readonly UserManager<IdentityUser> userManager;

        public CheckoutController(IConfiguration config, UserManager<IdentityUser> userManager, ApplicationDbContext db)
        {
            this.config = config;
            this.userManager = userManager;
            this.db = db;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var cartItems = await db.Cart
                    .Include(c => c.Product)
                    .Where(c => c.ApplicationUserId == user.Id)
                    .ToListAsync();

                if (cartItems.Count == 0)
                {
                    TempData["Error"] = "Your cart is empty.";
                    return RedirectToAction("Index", "Cart");
                }

                return View(cartItems);
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while loading the checkout page.";
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCheckoutSession()
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var cartItems = await db.Cart
                    .Include(c => c.Product)
                    .Where(c => c.ApplicationUserId == user.Id)
                    .ToListAsync();

                if (!cartItems.Any())
                {
                    TempData["Error"] = "Your cart is empty.";
                    return RedirectToAction("Index", "Cart");
                }

                foreach (var item in cartItems)
                {
                    if (item.Quantity > item.Product.Quantity)
                    {
                        TempData["Error"] = $"Only {item.Product.Quantity} left in stock for {item.Product.Title}.";
                        return RedirectToAction("Index", "Cart");
                    }
                }

                var domain = $"{Request.Scheme}://{Request.Host}";

                var lineItems = cartItems.Select(item => new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.Price), // Assuming already in cents
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
                    SuccessUrl = domain + "/Checkout/Success",
                    CancelUrl = domain + "/Cart/Index"
                };

                var service = new SessionService();
                var session = service.Create(options);

                return Redirect(session.Url);
            }
            catch (Exception)
            {
                TempData["Error"] = "Failed to create payment session.";
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Success()
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var cartItems = await db.Cart
                    .Include(c => c.Product)
                    .Where(c => c.ApplicationUserId == user.Id)
                    .ToListAsync();

                foreach (var item in cartItems)
                {
                    item.Product.Quantity -= item.Quantity;

                    var order = new Order
                    {
                        ApplicationUserId = user.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Product.Price,
                        Status = "Confirmed",
                        OrderDate = DateTime.Now
                    };

                    db.Order.Add(order);
                }

                db.Cart.RemoveRange(cartItems);
                await db.SaveChangesAsync();

                TempData["Success"] = "Order placed successfully!";
                return View();
            }
            catch (Exception)
            {
                TempData["Error"] = "Something went wrong while processing your order.";
                return RedirectToAction("Index", "Cart");
            }
        }

        public IActionResult Cancel()
        {
            TempData["Error"] = "Payment was cancelled.";
            return RedirectToAction("Index", "Cart");
        }
    }
}
