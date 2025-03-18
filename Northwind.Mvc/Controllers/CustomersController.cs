using Microsoft.AspNetCore.Mvc;
using Northwind.EntityModels;
using Northwind.Repositories;

namespace Northwind.Mvc.Controllers;

public class CustomersController : Controller
{
    private readonly ICustomerRepository _repo;

    public CustomersController(ICustomerRepository customerRepository)
    {
       _repo = customerRepository;
    }
    
    [Route("Customers/{country?}")]
    public async Task<IActionResult> Index(string? country = null)
    {
        IEnumerable<Customer> model = await _repo.RetrieveAllAsync();
        if (!string.IsNullOrWhiteSpace(country))
        {
            model = model.Where(customer => string.Equals(customer.Country, country, StringComparison.OrdinalIgnoreCase));
        }
        return View(model);
    }
}