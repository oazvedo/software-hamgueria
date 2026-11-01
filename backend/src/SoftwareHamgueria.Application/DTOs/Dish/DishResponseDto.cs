using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.Application.DTOs.Dish;

public record DishResponseDto(Guid Id, string Name, string Description, decimal Price, Category Category, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt);
