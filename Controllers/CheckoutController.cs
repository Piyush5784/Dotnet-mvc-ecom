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

namespace VMart.Controllers
{
    [Authorize(Roles = SD.Role_User)]
    public class CheckoutController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public CheckoutController(
            IConfiguration config,
            UserManager<IdentityUser> userManager,
            ApplicationDbContext db)
        {
            _config      = config;
            _userManager = userManager;
            _db          = db;
        }

        // GET: /Checkout
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) 
                return Unauthorized();
            var applicationUser = await _db.ApplicationUser.FindAsync(user.Id);
            if (applicationUser == null)
            {
                return Unauthorized();
            }

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
                City          = applicationUser.City,
                State         = applicationUser.State,
                PostalCode    = applicationUser.PostalCode,
                CartItems     = cartItems
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCheckoutSession(CheckoutViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) 
                return Unauthorized();
            var applicationUser = await _db.ApplicationUser.FindAsync(user.Id);
            if (applicationUser == null)
            {
                return Unauthorized();
            }

            var cartItems = await _db.Cart
                .Include(c => c.Product)
                .Where(c => c.ApplicationUserId == user.Id)
                .ToListAsync();

            model.CartItems = cartItems;

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill out all address fields.";
                return View("Index", model);
            }

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            // update user address
            applicationUser.StreetAddress = model.StreetAddress;
            applicationUser.City          = model.City;
            applicationUser.State         = model.State;
            applicationUser.PostalCode    = model.PostalCode;


            await _db.SaveChangesAsync();

            // validate stock
            foreach (var item in cartItems)
            {
                if (item.Quantity > item.Product.Quantity)
                {
                    TempData["Error"] = 
                        $"Only {item.Product.Quantity} left for {item.Product.Title}.";
                    return RedirectToAction("Index", "Cart");
                }
            }

            // build Stripe session
            var domain = $"{Request.Scheme}://{Request.Host}";
            var lineItems = cartItems.Select(item => new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(item.Product.Price * 100),
                    Currency   = "usd",
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
                LineItems          = lineItems,
                Mode               = "payment",
                SuccessUrl         = $"{domain}/Checkout/Success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl          = $"{domain}/Checkout/Cancel?session_id={{CHECKOUT_SESSION_ID}}"
            };

            var service = new SessionService();
            var session = service.Create(options);

            // nothing saved to orders yet—will happen in Success()
            return Redirect(session.Url);
        }


        [HttpGet]
        public async Task<IActionResult> Success(string session_id)
        {
            if (string.IsNullOrEmpty(session_id))
                return BadRequest();

            // retrieve the Stripe session to confirm payment
            var stripeSession = new SessionService().Get(session_id);
            if (stripeSession.PaymentStatus != "paid")
            {
                TempData["Error"] = "Payment not completed.";
                return RedirectToAction("Index", "Cart");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) 
                return Unauthorized();

            // grab current cart items
            var cartItems = await _db.Cart
                .Include(c => c.Product)
                .Where(c => c.ApplicationUserId == user.Id)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            // create orders now that payment succeeded
            foreach (var item in cartItems)
            {
                var order = new Order
                {
                    ApplicationUserId = user.Id,
                    ProductId         = item.ProductId,
                    Quantity          = item.Quantity,
                    Price             = item.Product.Price,
                    Status            = SD.OrderStatusConfirmed,
                    paymentStatus     = SD.Payment_Status_Completed,
                    PaymentSessionId  = session_id,
                    OrderDate         = DateTime.Now
                };
                _db.Order.Add(order);

                // decrement stock
                item.Product.Quantity -= item.Quantity;
            }

            // clear the cart
            _db.Cart.RemoveRange(cartItems);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Payment successful! Your order is confirmed.";
            return View();
        }

        // GET: /Checkout/Cancel
        [HttpGet]
        public IActionResult Cancel(string session_id)
        {
            // you could update any pending orders here if you pre-created them
            TempData["Error"] = "Payment was cancelled.";
            return RedirectToAction("Index", "Cart");
        }
    }
}
