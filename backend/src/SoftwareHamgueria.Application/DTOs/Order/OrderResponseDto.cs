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
    DateTime UpdatedAt);

public record OrderItemResponseDto(Guid DishId, string DishName, double Price, int Quantity, string Notes);

public record CloseComandaDto(Guid TableId, string CustomerName);
