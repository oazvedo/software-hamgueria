using FluentValidation;
using SoftwareHamgueria.Application.DTOs.Order;

namespace SoftwareHamgueria.Application.Validators.Order;

public class CloseComandaDtoValidator : AbstractValidator<CloseComandaDto>
{
    public CloseComandaDtoValidator()
    {
        RuleFor(x => x.TableId).TableChosen();
        RuleFor(x => x.CustomerName).ComandaName();
    }
}
