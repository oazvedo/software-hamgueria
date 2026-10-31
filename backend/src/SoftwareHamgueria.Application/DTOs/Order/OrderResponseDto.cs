using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.Application.DTOs.Order;

public record OrderResponseDto(
    Guid Id,
    Guid TableId,
    string TableName,
    string CustomerName,
    List<OrderItemResponseDto> Items,
    OrderStatus Status,
    bool ComandaClosed,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    // Only in the response that creates the order (ADR 0030): the client keeps it to change or cancel the order later.
    string? OwnerToken = null);

public record OrderItemResponseDto(Guid DishId, string DishName, double Price, int Quantity, string Notes);

public record CloseComandaDto(Guid TableId, string CustomerName);
