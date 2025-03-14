using Northwind.EntityModels;

namespace Northwind.Mvc.Models;

public record HomeSupplierViewModel(int EntitiesAffected, Supplier? Supplier);