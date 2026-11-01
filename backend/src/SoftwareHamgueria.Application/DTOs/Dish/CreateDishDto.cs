using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.Application.DTOs.Dish;

public record CreateDishDto(string Name, string Description, decimal Price, Category Category);
