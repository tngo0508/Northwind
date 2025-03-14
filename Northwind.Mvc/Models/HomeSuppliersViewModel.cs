using Northwind.EntityModels;
namespace Northwind.Mvc.Models;

public record HomeSuppliersViewModel(IEnumerable<Supplier>? Suppliers);