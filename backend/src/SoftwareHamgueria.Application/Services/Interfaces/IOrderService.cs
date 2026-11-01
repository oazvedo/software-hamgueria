using SoftwareHamgueria.Application.Orders;
using SoftwareHamgueria.Application.DTOs.Order;
using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.Application.Services.Interfaces;

public interface IOrderService
{
    Task<OrderResponseDto> CreateAsync(CreateOrderDto dto);
    Task<OrderResponseDto> UpdateAsync(Guid id, UpdateOrderDto dto, OrderActor actor);
    Task<OrderResponseDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<OrderResponseDto>> GetAllAsync();
    Task<IEnumerable<OrderResponseDto>> GetComandaAsync(Guid tableId, string customerName);
    Task<IEnumerable<OrderResponseDto>> GetTableOrdersAsync(Guid tableId);
    Task<IReadOnlyList<IdleComandaDto>> GetIdleComandasAsync(int hours);
    Task CancelAsync(Guid id, OrderActor actor);
    Task UpdateStatusAsync(Guid id, OrderStatus status);
    Task CloseComandaAsync(CloseComandaDto dto);
}
