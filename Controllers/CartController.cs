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

namespace VMart.Controllers
{
    [Authorize(Roles = SD.Role_User)]
    public class CartController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly ApplicationDbContext db;
        private readonly ILogService logEntry;

        public CartController(UserManager<IdentityUser> userManager, ApplicationDbContext db,ILogService logEntry)
        {
            this.logEntry = logEntry;
            this.userManager = userManager;
            this.db = db;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized();

                var cartItems = await db.Cart
                    .Include(c => c.Product)
                    .Where(c => c.ApplicationUserId == user.Id)
                    .ToListAsync();

                return View(cartItems);
            }
            catch (Exception ex)
            {
                await logEntry.LogAsync(SD.Log_Error, "Failed to load cart", "Cart", "Index", ex.ToString(), Request.Path, User.Identity?.Name);
                TempData["Error"] = "Failed to load your cart.";
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddToCart(int? id)
        {
            try
            {
                if (id == null) return BadRequest();

                var user = await userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var product = await db.Product.FindAsync(id);
                if (product == null) return NotFound();

                var existingCartItem = await db.Cart
                    .FirstOrDefaultAsync(c => c.ProductId == id && c.ApplicationUserId == user.Id);

                if (existingCartItem != null)
                {
                    existingCartItem.Quantity += 1;
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index", "Cart");
                }

                var cartItem = new Cart
                {
                    ProductId = product.Id,
                    ApplicationUserId = user.Id,
                    Quantity = 1
                };

                await db.Cart.AddAsync(cartItem);
                await db.SaveChangesAsync();

                TempData["Success"] = "Item added to cart!";
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

                var user = await userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var existingCartItem = await db.Cart
                    .FirstOrDefaultAsync(c => c.ProductId == id && c.ApplicationUserId == user.Id);

                if (existingCartItem != null)
                {
                    db.Cart.Remove(existingCartItem);
                    await db.SaveChangesAsync();
                    TempData["Success"] = "Item removed from cart.";
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

                var user = await userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var existingCartItem = await db.Cart
                    .FirstOrDefaultAsync(c => c.ProductId == id && c.ApplicationUserId == user.Id);

                if (existingCartItem != null)
                {
                    existingCartItem.Quantity -= 1;
                    if (existingCartItem.Quantity <= 0)
                        db.Cart.Remove(existingCartItem);

                    await db.SaveChangesAsync();
                    TempData["Success"] = "Item quantity decreased.";
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

                var user = await userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var existingCartItem = await db.Cart
                    .FirstOrDefaultAsync(c => c.ProductId == id && c.ApplicationUserId == user.Id);

                if (existingCartItem == null)
                {
                    TempData["Error"] = "Item not found in cart.";
                    return RedirectToAction("Index", "Cart");
                }

                existingCartItem.Quantity += 1;
                await db.SaveChangesAsync();

                TempData["Success"] = "Item quantity increased.";
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
