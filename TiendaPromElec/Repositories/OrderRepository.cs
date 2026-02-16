using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;

namespace TiendaPromElec.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.Customer) // Traer datos del cliente
            .Include(o => o.Items)
            .ThenInclude(i=>i.Product)
            .OrderByDescending(o=>o.OrderDate)// Traer los items de la orden
            .ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(long id)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .ThenInclude(od => od.Product) // Incluso traer el producto dentro del detalle
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync(long customerId)
    {
        return await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.Items)
            .ThenInclude(od => od.Product)
            .ToListAsync();
    }

    public async Task AddAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Order order)
    {
        _context.Entry(order).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order != null)
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(long id)
    {
        return await _context.Orders.AnyAsync(e => e.Id == id);
    }
}
