using FluentValidation;
using SoftwareHamgueria.Application.DTOs.Order;

namespace SoftwareHamgueria.Application.Validators.Order;

public class UpdateOrderDtoValidator : AbstractValidator<UpdateOrderDto>
{
    public UpdateOrderDtoValidator()
    {
        RuleFor(x => x.Items).Custom(OrderItemsRules.Validate);
    }
}
