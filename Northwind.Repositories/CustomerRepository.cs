using Microsoft.EntityFrameworkCore; // To use ToArrayAsync
using Microsoft.EntityFrameworkCore.ChangeTracking; // To use EntityEntry<T>
using Microsoft.Extensions.Caching.Hybrid; // To use HybridCache
using Northwind.EntityModels; // To use Customer

namespace Northwind.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly NorthwindContext _db;
    private readonly HybridCache _cache;

    public CustomerRepository(NorthwindContext db, HybridCache hybridCache)
    {
        _db = db;
        _cache = hybridCache;
    }
    public async Task<Customer?> CreateAsync(Customer c)
    {
        c.CustomerId = c.CustomerId.ToUpper(); // Normalize to uppercase.
        
        // Add to database using EF Core.
        EntityEntry<Customer> added = await _db.Customers.AddAsync(c);
        int affected = await _db.SaveChangesAsync();
        if (affected == 1)
        {
            // If saved to database then store in cache
            await _cache.SetAsync(c.CustomerId, c);
            return c;
        }

        return null;
    }

    public async Task<Customer[]> RetrieveAllAsync()
    {
        return await _db.Customers.ToArrayAsync();
    }

    public async Task<Customer?> RetrieveAsync(string id, CancellationToken token = default)
    {
        id = id.ToUpper(); // Normalize to uppercase
        return await _cache.GetOrCreateAsync(
            key: id,
            factory: async cancel => await _db.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == id, token),
            cancellationToken: token);
    }

    public async Task<Customer?> UpdateAsync(Customer c)
    {
        c.CustomerId = c.CustomerId.ToUpper();
        _db.Customers.Update(c);
        int affected = await _db.SaveChangesAsync();
        if (affected == 1)
        {
            await _cache.SetAsync(c.CustomerId, c);
            return c;
        }

        return null;
    }

    public async Task<bool?> DeleteAsync(string id)
    {
        id = id.ToUpper();
        Customer? c = await _db.Customers.FindAsync(id);
        if (c is null) return null;
        _db.Customers.Remove(c);
        int affected = await _db.SaveChangesAsync();
        if (affected == 1)
        {
            await _cache.RemoveAsync(c.CustomerId);
            return true;
        }

        return null;
    }
}