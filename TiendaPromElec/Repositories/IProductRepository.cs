using ProductApi.Models;

namespace TiendaPromElec.Repositories;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(long id);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(long id);
    Task<bool> ExistsAsync(long id);
}
