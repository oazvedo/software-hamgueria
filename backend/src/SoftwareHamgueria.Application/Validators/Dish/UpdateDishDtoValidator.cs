using FluentValidation;
using SoftwareHamgueria.Application.DTOs.Dish;

namespace SoftwareHamgueria.Application.Validators.Dish;

public class UpdateDishDtoValidator : AbstractValidator<UpdateDishDto>
{
    public UpdateDishDtoValidator()
    {
        RuleFor(x => x.Name).DishName();
        RuleFor(x => x.Description).DishDescription();
        RuleFor(x => x.Price).DishPrice();
        RuleFor(x => x.Category).DishCategory();
    }
}
