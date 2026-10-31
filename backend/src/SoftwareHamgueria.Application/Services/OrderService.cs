using FluentValidation;
using SoftwareHamgueria.Application.DTOs.Order;
using SoftwareHamgueria.Application.Exceptions;
using SoftwareHamgueria.Application.Messaging;
using SoftwareHamgueria.Application.Repositories;
using SoftwareHamgueria.Application.Services.Interfaces;
using SoftwareHamgueria.Domain.Entities;
using SoftwareHamgueria.Application.Telemetry;

namespace SoftwareHamgueria.Application.Services;

public class OrderService(
    IOrderRepository orderRepository,
    IDishRepository dishRepository,
    ITableRepository tableRepository,
    IComandaRepository comandaRepository,
    IOrderEventPublisher publisher,
    IValidator<CreateOrderDto> createValidator,
    IValidator<UpdateOrderDto> updateValidator,
    IValidator<CloseComandaDto> closeValidator,
    IValidator<ComandaQuery> comandaQueryValidator) : IOrderService
{
    public async Task<OrderResponseDto> CreateAsync(CreateOrderDto dto)
    {
        await createValidator.ValidateAndThrowAsync(dto);

        _ = await tableRepository.GetByIdAsync(dto.TableId)
            ?? throw new NotFoundException("A mesa informada não existe.");

        var order = new Order(dto.TableId, dto.CustomerName.Trim());
        await AddItemsAsync(order, dto.Items);

        await orderRepository.AddAsync(order);
        var saved = await orderRepository.GetByIdAsync(order.Id);
        publisher.Publish("order.created", order.Id.ToString());
        HamgueriaTelemetry.OrdersCreated.Add(1);
        // A brand-new order always reopens the comanda, so it is never closed.
        return ToDto(saved!, false);
    }

    public async Task<OrderResponseDto> UpdateAsync(Guid id, UpdateOrderDto dto)
    {
        await updateValidator.ValidateAndThrowAsync(dto);

        var order = await orderRepository.GetByIdAsync(id) ?? throw OrderNotFound();

        order.Items.Clear();
        await AddItemsAsync(order, dto.Items);

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
        await comandaQueryValidator.ValidateAndThrowAsync(new ComandaQuery(tableId, customerName));
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
        var order = await orderRepository.GetByIdAsync(id) ?? throw OrderNotFound();
        order.Cancel();
        await orderRepository.UpdateAsync(order);
        publisher.Publish("order.cancelled", order.Id.ToString());
    }

    public async Task UpdateStatusAsync(Guid id, OrderStatus status)
    {
        var order = await orderRepository.GetByIdAsync(id) ?? throw OrderNotFound();
        order.UpdateStatus(status);
        await orderRepository.UpdateAsync(order);
    }

    public async Task CloseComandaAsync(CloseComandaDto dto)
    {
        await closeValidator.ValidateAndThrowAsync(dto);
        await comandaRepository.UpsertAsync(dto.TableId, dto.CustomerName.Trim());
    }

    private async Task AddItemsAsync(Order order, IEnumerable<OrderItemDto> items)
    {
        foreach (var item in items)
        {
            var dish = await dishRepository.GetByIdAsync(item.DishId)
                ?? throw new NotFoundException("O prato informado não existe.");
            if (!dish.IsActive) throw new BusinessRuleException($"O prato {dish.Name} não está disponível.");
            order.AddItem(item.DishId, item.Quantity, item.Notes ?? string.Empty);
        }
    }

    private static NotFoundException OrderNotFound() => new("Pedido não encontrado.");

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
