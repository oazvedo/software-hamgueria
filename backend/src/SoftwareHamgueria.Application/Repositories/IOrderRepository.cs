using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.Application.Repositories;

public interface IOrderRepository
{
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task<Order?> GetByIdAsync(Guid id);
    /// <summary>Orders still in progress (any age) plus those finished since <paramref name="finishedSince"/>, newest first, at most <paramref name="max"/>.</summary>
    Task<IEnumerable<Order>> GetForKitchenAsync(DateTime finishedSince, int max);
    Task<IEnumerable<Order>> GetByComandaAsync(Guid tableId, string customerName);
    Task<IEnumerable<Order>> GetByTableAsync(Guid tableId);
}
