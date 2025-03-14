using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Northwind.EntityModels;
using Northwind.Mvc.Models;
using Microsoft.EntityFrameworkCore;

namespace Northwind.Mvc.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly NorthwindContext _db;

    public HomeController(ILogger<HomeController> logger, NorthwindContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        _logger.LogError("This is a serious error");
        _logger.LogWarning("This is a warning");
        _logger.LogWarning("Second warning!");
        _logger.LogInformation("This is an information");
        HomeIndexViewModel model = new(
            VisitorCount: Random.Shared.Next(1, 1001), 
            Categories: await _db.Categories.ToListAsync(),
            Products: await _db.Products.ToListAsync());
        return View(model);
    }

    [Route("private")]
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public async Task<IActionResult> ProductDetail(int? id)
    {
        if (!id.HasValue)
        {
            return BadRequest("You must pass a product ID in the route, for example, /Home/ProductDetail/21");
        }

        Product? model = await _db.Products.Include(p => p.Category)
            .SingleOrDefaultAsync(p => p.ProductId == id);

        if (model is null)
        {
            return NotFound($"ProductId {id} not found");
        }

        return View(model);
    }
    
    // This action method will handle GET and other requests except POST
    public IActionResult ModelBinding()
    {
        return View();
    }

    [HttpPost] // This action method will handle POST request.
    public IActionResult ModelBinding(Thing? thing)
    {
        HomeModelBindingViewModel model = new(
            Thing: thing,
            HasErrors: !ModelState.IsValid,
            ValidationErrors: ModelState.Values.SelectMany(state => state.Errors).Select(error => error.ErrorMessage));

        return View(model); // Show the model bound thing.
    }
    
    // This action method will handle requests to display all suppliers.
    public IActionResult Suppliers()
    {
        HomeSuppliersViewModel model = new(_db.Suppliers.OrderBy(c => c.Country).ThenBy(c => c.CompanyName));
        return View(model);
    }
    
    // GET: /home/editsupplier/{id}
    public IActionResult EditSupplier(int? id)
    {
        Supplier? supplierInDb = _db.Suppliers.Find(id);
        HomeSupplierViewModel model = new(supplierInDb is null ? 0 : 1, supplierInDb);
        
        // Views\Home\EditSupplier.cshtml
        return View(model);
    }
    
    // POST: /home/editsupplier
    // body: JSON Supplier
    // Updates an existing supplier
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditSupplier(Supplier supplier)
    {
        int affected = 0;
        if (ModelState.IsValid)
        {
            Supplier? supplierInDb = _db.Suppliers.Find(supplier.SupplierId);

            if (supplierInDb is not null)
            {
                supplierInDb.CompanyName = supplier.CompanyName;
                supplierInDb.Country = supplier.Country;
                supplierInDb.Phone = supplier.Phone;
            }
            
            affected = _db.SaveChanges();
        }

        HomeSupplierViewModel model = new(affected, supplier);

        if (affected == 0)
        {
            return View(model);
        }
        else
        {
            return RedirectToAction("Suppliers");
        }
    }
    
    // GET: /home/deletesupplier/{id}
    public IActionResult DeleteSupplier(int? id)
    {
        Supplier? supplierInDb = _db.Suppliers.Find(id);
        HomeSupplierViewModel model = new(supplierInDb is null ? 0 : 1, supplierInDb);
        return View(model);
    }

    [HttpPost("/home/deletesupplier/{id:int}")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteSupplierX(int? id)
    {
        int affected = 0;
        Supplier? supplierInDb = _db.Suppliers.Find(id);
        if (supplierInDb is not null)
        {
            _db.Suppliers.Remove(supplierInDb);
            affected = _db.SaveChanges();
        }
        
        HomeSupplierViewModel model = new(affected, supplierInDb);

        if (affected == 0)
        {
            return View("DeleteSupplier",model);
        }
        
        return RedirectToAction("Suppliers");
    }

    public IActionResult AddSupplier()
    {
        HomeSupplierViewModel model = new(0, new Supplier());
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddSupplier(Supplier supplier)
    {
        int affected = 0;
        if (ModelState.IsValid)
        {
            _db.Suppliers.Add(supplier);
            affected = _db.SaveChanges();
        }
        
        HomeSupplierViewModel model = new(affected, supplier);
        if (affected == 0)
        {
            return View(model);
        }
        return RedirectToAction("Suppliers");
    }

    public IActionResult ProductsThatCostMoreThan(decimal? price)
    {
        if (!price.HasValue)
        {
            return BadRequest("You must pass a product price in the query string");
        }

        IEnumerable<Product> model = _db.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.UnitPrice > price);

        if (!model.Any())
        {
            return NotFound($"No Products cost more than {price:C}");
        }

        ViewData["MaxPrice"] = price.Value.ToString("C");
        return View("Views/Home/CostlyProducts.cshtml", model);
    }
}