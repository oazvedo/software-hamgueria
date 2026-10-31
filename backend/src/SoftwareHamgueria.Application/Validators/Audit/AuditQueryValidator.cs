using FluentValidation;
using SoftwareHamgueria.Application.DTOs.Audit;

namespace SoftwareHamgueria.Application.Validators.Audit;

public class AuditQueryValidator : AbstractValidator<AuditQuery>
{
    /// <summary>The tables with the audit trigger (migration AddAuditTrail).</summary>
    public static readonly string[] AuditedTables = ["Orders", "OrderItems", "Dishes", "Tables", "ComandaClosures"];

    public AuditQueryValidator()
    {
        RuleFor(x => x.Table)
            .Must(t => t is null || AuditedTables.Contains(t))
            .WithMessage($"A tabela precisa ser uma de: {string.Join(", ", AuditedTables)}.");
        RuleFor(x => x.RowId).MaximumLength(64).WithMessage("O registro pode ter no máximo 64 caracteres.");
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage("A página começa em 1.");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200).WithMessage("O tamanho da página vai de 1 a 200.");
    }
}
