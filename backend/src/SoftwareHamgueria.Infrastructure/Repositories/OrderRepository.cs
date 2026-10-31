using Microsoft.EntityFrameworkCore;
using SoftwareHamgueria.Application.Repositories;
using SoftwareHamgueria.Domain.Entities;
using SoftwareHamgueria.Infrastructure.Data;

namespace SoftwareHamgueria.Infrastructure.Repositories;

public class OrderRepository(AppDbContext context) : IOrderRepository
{
    public async Task AddAsync(Order order)
    {
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Order order)
    {
        context.Orders.Update(order);
        await context.SaveChangesAsync();
    }

    public Task<Order?> GetByIdAsync(Guid id) =>
        context.Orders
            .Include(o => o.Table)
            .Include(o => o.Items).ThenInclude(i => i.Dish)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<IEnumerable<Order>> GetForKitchenAsync(DateTime finishedSince, int max) =>
        await context.Orders
            .Include(o => o.Table)
            .Include(o => o.Items).ThenInclude(i => i.Dish)
            .Where(o => (o.Status != OrderStatus.Done && o.Status != OrderStatus.Cancelled) || o.UpdatedAt >= finishedSince)
            .OrderByDescending(o => o.UpdatedAt)
            .Take(max)
            .AsSplitQuery()
            .ToListAsync();

    public async Task<IEnumerable<Order>> GetByComandaAsync(Guid tableId, string customerName) =>
        await context.Orders
            .Include(o => o.Table)
            .Include(o => o.Items).ThenInclude(i => i.Dish)
            .Where(o => o.TableId == tableId && o.CustomerName.ToLower() == customerName.ToLower())
            .OrderBy(o => o.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Order>> GetByTableAsync(Guid tableId) =>
        await context.Orders
            .Include(o => o.Table)
            .Include(o => o.Items).ThenInclude(i => i.Dish)
            .Where(o => o.TableId == tableId)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync();
}
