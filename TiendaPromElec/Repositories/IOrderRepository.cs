using ProductApi.Models;

namespace TiendaPromElec.Repositories;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(long id);
    Task<IEnumerable<Order>> GetOrdersByCustomerAsync(long customerId); // Método extra útil
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task DeleteAsync(long id);
    Task<bool> ExistsAsync(long id);
}
