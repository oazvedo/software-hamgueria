using FluentValidation;
using SoftwareHamgueria.Application.DTOs.Order;

namespace SoftwareHamgueria.Application.Validators.Order;

public class ComandaQueryValidator : AbstractValidator<ComandaQuery>
{
    public ComandaQueryValidator()
    {
        RuleFor(x => x.TableId).TableChosen();
        RuleFor(x => x.Customer).ComandaName();
    }
}
