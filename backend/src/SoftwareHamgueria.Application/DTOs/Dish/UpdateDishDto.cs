using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.Application.DTOs.Dish;

public record UpdateDishDto(string Name, string Description, double Price, Category Category);
