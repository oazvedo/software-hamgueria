using SoftwareHamgueria.Application.DTOs.Dish;

namespace SoftwareHamgueria.Application.Services.Interfaces;

public interface IDishService
{
    Task<DishResponseDto> CreateAsync(CreateDishDto dto);
    Task<DishResponseDto> UpdateAsync(Guid id, UpdateDishDto dto);
    Task<DishResponseDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<DishResponseDto>> GetAllAsync(bool onlyActive = false);
    Task DisableAsync(Guid id);
    Task EnableAsync(Guid id);
}
