using Microsoft.AspNetCore.Mvc;
using VMart.Data;
using VMart.Models;
using Microsoft.EntityFrameworkCore;
using VMart.Models.ViewModels;

namespace VMart.Controllers
{
    public class LatestProductController : Controller
    {
        private readonly ApplicationDbContext db;

        public LatestProductController(ApplicationDbContext db)
        {
            this.db = db;
        }


        public async Task<IActionResult> Index()
        {

            List<LatestProduct> products = await db.LatestProduct
                   .Include(lp => lp.Product)
                   .OrderBy(lp => lp.DisplayOrder)
                   .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> Create()
        {

            var addModel = new AddLatestProductViewModel
            {
                Products = await db.Product.ToListAsync(),
                NewProduct = new LatestProduct()
            };

            return View(addModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddLatestProductViewModel data)
        {
            if (!ModelState.IsValid)
            {
                data.Products = await db.Product.ToListAsync();
                return View(data);
            }

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

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Update(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

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

            return View(AddModel);
        }

        [HttpPost]
        public async Task<IActionResult> Update(AddLatestProductViewModel data)
        {
            if (!ModelState.IsValid)
            {
                data.Products = await db.Product.ToListAsync();
                return View(data);
            }

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

            return RedirectToAction("Index");
        }



        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

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

            return View(AddModel);
        }

        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteLatestProduct(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var product = await db.LatestProduct.FindAsync(id);

            if (product is null)
            {
                return NotFound();
            }
            
            db.LatestProduct.Remove(product);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }



    }
}
