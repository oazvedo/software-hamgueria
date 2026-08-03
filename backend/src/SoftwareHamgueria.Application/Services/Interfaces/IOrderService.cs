using SoftwareHamgueria.Application.DTOs.Order;
using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.Application.Services.Interfaces;

public interface IOrderService
{
    Task<OrderResponseDto> CreateAsync(CreateOrderDto dto);
    Task<OrderResponseDto> UpdateAsync(Guid id, UpdateOrderDto dto);
    Task<OrderResponseDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<OrderResponseDto>> GetAllAsync();
    Task<IEnumerable<OrderResponseDto>> GetComandaAsync(Guid tableId, string customerName);
    Task<IEnumerable<OrderResponseDto>> GetTableOrdersAsync(Guid tableId);
    Task CancelAsync(Guid id);
    Task UpdateStatusAsync(Guid id, OrderStatus status);
    Task CloseComandaAsync(Guid tableId, string customerName);
}
