using FluentValidation;
using SoftwareHamgueria.Application.DTOs.Order;

namespace SoftwareHamgueria.Application.Validators.Order;

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.TableId).TableChosen();
        RuleFor(x => x.CustomerName).ComandaName();
        RuleFor(x => x.Items).Custom(OrderItemsRules.Validate);
    }
}
