namespace SoftwareHamgueria.Application.DTOs.Order;

public record CreateOrderDto(Guid TableId, string CustomerName, List<OrderItemDto> Items);
public record OrderItemDto(Guid DishId, int Quantity, string? Notes = null);
