using FluentValidation;
using SoftwareHamgueria.Application.DTOs.Table;

namespace SoftwareHamgueria.Application.Validators.Table;

public class UpdateTableDtoValidator : AbstractValidator<UpdateTableDto>
{
    public UpdateTableDtoValidator()
    {
        RuleFor(x => x.TableName).RequiredTrimmed(Limits.TableName,
            "Informe o nome da mesa.",
            $"O nome da mesa pode ter no máximo {Limits.TableName} caracteres.");
    }
}
