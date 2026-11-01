using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.Application.Repositories;

public interface IOrderRepository
{
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task<Order?> GetByIdAsync(Guid id);
    /// <summary>Orders still in progress (any age) plus those finished since <paramref name="finishedSince"/>, newest first, at most <paramref name="max"/>.</summary>
    Task<IEnumerable<Order>> GetForKitchenAsync(DateTime finishedSince, int max);

    /// <summary>
    /// Every order (table, table name, customer name as typed, time) of the comandas whose last order is older than
    /// <paramref name="idleSince"/>. A comanda is table + name ignoring case (ADR 0014), like everywhere else.
    /// </summary>
    Task<IReadOnlyList<ComandaOrderRow>> GetOrdersOfComandasIdleSinceAsync(DateTime idleSince);
    Task<IEnumerable<Order>> GetByComandaAsync(Guid tableId, string customerName);
    Task<IEnumerable<Order>> GetByTableAsync(Guid tableId);
}

/// <summary>One order of a comanda, as the idle-comandas warning needs it (spec 025).</summary>
public record ComandaOrderRow(Guid TableId, string TableName, string CustomerName, DateTime CreatedAt);
