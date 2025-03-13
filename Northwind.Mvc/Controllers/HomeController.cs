using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Northwind.EntityModels;
using Northwind.Mvc.Models;

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

    public IActionResult Index()
    {
        _logger.LogError("This is a serious error");
        _logger.LogWarning("This is a warning");
        _logger.LogWarning("Second warning!");
        _logger.LogInformation("This is an information");
        HomeIndexViewModel model = new (VisitorCount: Random.Shared.Next(1, 1001), Categories: _db.Categories.ToList(), Products: _db.Products.ToList());
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
}