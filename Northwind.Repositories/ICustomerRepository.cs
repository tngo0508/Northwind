using Northwind.EntityModels;

namespace Northwind.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> CreateAsync(Customer c);
    Task<Customer[]> RetrieveAllAsync();
    Task<Customer?> RetrieveAsync(string id, CancellationToken token);
    Task<Customer?> UpdateAsync(Customer c);
    Task<bool?> DeleteAsync(string id);
}