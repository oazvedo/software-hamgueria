using SoftwareHamgueria.Application.DTOs.Dish;
using SoftwareHamgueria.Application.Repositories;
using SoftwareHamgueria.Application.Services.Interfaces;
using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.Application.Services;

public class DishService(IDishRepository repository) : IDishService
{
    public async Task<DishResponseDto> CreateAsync(CreateDishDto dto)
    {
        var dish = new Dish(dto.Name, dto.Description, dto.Price, dto.Category);
        await repository.AddAsync(dish);
        return ToDto(dish);
    }

    public async Task<DishResponseDto> UpdateAsync(Guid id, UpdateDishDto dto)
    {
        var dish = await repository.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Dish {id} not found.");
        dish.Update(dto.Name, dto.Description, dto.Price, dto.Category);
        await repository.UpdateAsync(dish);
        return ToDto(dish);
    }

    public async Task<DishResponseDto?> GetByIdAsync(Guid id)
    {
        var dish = await repository.GetByIdAsync(id);
        return dish is null ? null : ToDto(dish);
    }

    public async Task<IEnumerable<DishResponseDto>> GetAllAsync()
    {
        var dishes = await repository.GetAllAsync();
        return dishes.Select(ToDto);
    }

    public async Task DisableAsync(Guid id)
    {
        var dish = await repository.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Dish {id} not found.");
        dish.Disable();
        await repository.UpdateAsync(dish);
    }

    public async Task EnableAsync(Guid id)
    {
        var dish = await repository.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Dish {id} not found.");
        dish.Enable();
        await repository.UpdateAsync(dish);
    }

    private static DishResponseDto ToDto(Dish d) =>
        new(d.Id, d.Name, d.Description, d.Price, d.Category, d.IsActive, d.CreatedAt, d.UpdatedAt);
}
