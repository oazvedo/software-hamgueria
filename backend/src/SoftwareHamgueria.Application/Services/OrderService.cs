using SoftwareHamgueria.Application.DTOs.Order;
using SoftwareHamgueria.Application.Messaging;
using SoftwareHamgueria.Application.Repositories;
using SoftwareHamgueria.Application.Services.Interfaces;
using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.Application.Services;

public class OrderService(
    IOrderRepository orderRepository,
    IDishRepository dishRepository,
    ITableRepository tableRepository,
    IComandaRepository comandaRepository,
    IOrderEventPublisher publisher) : IOrderService
{
    public async Task<OrderResponseDto> CreateAsync(CreateOrderDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CustomerName))
            throw new InvalidOperationException("Customer name (comanda) is required.");

        _ = await tableRepository.GetByIdAsync(dto.TableId)
            ?? throw new KeyNotFoundException($"Table {dto.TableId} not found.");

        var order = new Order(dto.TableId, dto.CustomerName.Trim());

        foreach (var item in dto.Items)
        {
            var dish = await dishRepository.GetByIdAsync(item.DishId)
                ?? throw new KeyNotFoundException($"Dish {item.DishId} not found.");
            if (!dish.IsActive) throw new InvalidOperationException($"Dish '{dish.Name}' is not active.");
            order.AddItem(item.DishId, item.Quantity, item.Notes ?? string.Empty);
        }

        await orderRepository.AddAsync(order);
        var saved = await orderRepository.GetByIdAsync(order.Id);
        publisher.Publish("order.created", order.Id.ToString());
        // A brand-new order always reopens the comanda, so it is never closed.
        return ToDto(saved!, false);
    }

    public async Task<OrderResponseDto> UpdateAsync(Guid id, UpdateOrderDto dto)
    {
        var order = await orderRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Order {id} not found.");

        order.Items.Clear();
        foreach (var item in dto.Items)
        {
            var dish = await dishRepository.GetByIdAsync(item.DishId)
                ?? throw new KeyNotFoundException($"Dish {item.DishId} not found.");
            if (!dish.IsActive) throw new InvalidOperationException($"Dish '{dish.Name}' is not active.");
            order.AddItem(item.DishId, item.Quantity, item.Notes ?? string.Empty);
        }

        order.UpdateStatus(OrderStatus.Draft);
        await orderRepository.UpdateAsync(order);
        var saved = await orderRepository.GetByIdAsync(order.Id);
        publisher.Publish("order.updated", order.Id.ToString());
        return ToDto(saved!, false);
    }

    public async Task<OrderResponseDto?> GetByIdAsync(Guid id)
    {
        var order = await orderRepository.GetByIdAsync(id);
        if (order is null) return null;
        var closure = await comandaRepository.GetAsync(order.TableId, order.CustomerName);
        return ToDto(order, IsClosed(order, closure?.ClosedAt));
    }

    public async Task<IEnumerable<OrderResponseDto>> GetAllAsync()
    {
        var orders = await orderRepository.GetAllAsync();
        var closures = BuildClosureMap(await comandaRepository.GetAllAsync());
        return orders.Select(o => ToDto(o, IsClosed(o, closures)));
    }

    public async Task<IEnumerable<OrderResponseDto>> GetComandaAsync(Guid tableId, string customerName)
    {
        var orders = await orderRepository.GetByComandaAsync(tableId, customerName);
        var closure = await comandaRepository.GetAsync(tableId, customerName);
        return orders.Select(o => ToDto(o, IsClosed(o, closure?.ClosedAt)));
    }

    public async Task<IEnumerable<OrderResponseDto>> GetTableOrdersAsync(Guid tableId)
    {
        var orders = await orderRepository.GetByTableAsync(tableId);
        var closures = BuildClosureMap(await comandaRepository.GetForTableAsync(tableId));
        return orders.Select(o => ToDto(o, IsClosed(o, closures)));
    }

    public async Task CancelAsync(Guid id)
    {
        var order = await orderRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Order {id} not found.");
        order.Cancel();
        await orderRepository.UpdateAsync(order);
        publisher.Publish("order.cancelled", order.Id.ToString());
    }

    public async Task UpdateStatusAsync(Guid id, OrderStatus status)
    {
        var order = await orderRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Order {id} not found.");
        order.UpdateStatus(status);
        await orderRepository.UpdateAsync(order);
    }

    public async Task CloseComandaAsync(Guid tableId, string customerName)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new InvalidOperationException("Customer name (comanda) is required.");
        await comandaRepository.UpsertAsync(tableId, customerName.Trim());
    }

    private static Dictionary<(Guid, string), DateTime> BuildClosureMap(IEnumerable<ComandaClosure> closures)
    {
        var map = new Dictionary<(Guid, string), DateTime>();
        foreach (var c in closures)
            map[(c.TableId, c.CustomerName.ToLowerInvariant())] = c.ClosedAt;
        return map;
    }

    private static bool IsClosed(Order o, IReadOnlyDictionary<(Guid, string), DateTime> closures) =>
        closures.TryGetValue((o.TableId, o.CustomerName.ToLowerInvariant()), out var closedAt)
        && IsClosed(o, closedAt);

    // A comanda is closed for this order if it was closed at/after the order was placed.
    private static bool IsClosed(Order o, DateTime? closedAt) => closedAt is not null && o.CreatedAt <= closedAt.Value;

    private static OrderResponseDto ToDto(Order o, bool comandaClosed) =>
        new(o.Id, o.TableId, o.Table?.TableName ?? string.Empty, o.CustomerName,
            o.Items.Select(i => new OrderItemResponseDto(i.DishId, i.Dish?.Name ?? string.Empty, i.Dish?.Price ?? 0, i.Quantity, i.Notes)).ToList(),
            o.Status, comandaClosed, o.CreatedAt, o.UpdatedAt);
}
