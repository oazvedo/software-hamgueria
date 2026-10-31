using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.Application.Repositories;

public interface IOrderRepository
{
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task<Order?> GetByIdAsync(Guid id);
    /// <summary>Orders still in progress (any age) plus those finished since <paramref name="finishedSince"/>, newest first, at most <paramref name="max"/>.</summary>
    /// <summary>Per comanda (table + name), the last order time, count and table name; only comandas idle since <paramref name="idleSince"/>.</summary>
    Task<IReadOnlyList<(Guid TableId, string TableName, string CustomerName, DateTime LastOrderAt, int Orders)>> GetComandasIdleSinceAsync(DateTime idleSince);

    Task<IEnumerable<Order>> GetForKitchenAsync(DateTime finishedSince, int max);
    Task<IEnumerable<Order>> GetByComandaAsync(Guid tableId, string customerName);
    Task<IEnumerable<Order>> GetByTableAsync(Guid tableId);
}
