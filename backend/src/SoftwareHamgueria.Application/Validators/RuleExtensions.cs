using FluentValidation;

namespace SoftwareHamgueria.Application.Validators;

public static class RuleExtensions
{
    /// <summary>Required text, measured without the spaces at the ends (the services save it trimmed).</summary>
    public static IRuleBuilderOptions<T, string> RequiredTrimmed<T>(
        this IRuleBuilder<T, string> rule, int maxLength, string emptyMessage, string tooLongMessage) =>
        rule.Must(v => !string.IsNullOrWhiteSpace(v)).WithMessage(emptyMessage)
            .Must(v => v is null || v.Trim().Length <= maxLength).WithMessage(tooLongMessage);

    public static IRuleBuilderOptions<T, string> ComandaName<T>(this IRuleBuilder<T, string> rule) =>
        rule.RequiredTrimmed(Limits.CustomerName,
            "Informe o nome da comanda.",
            $"O nome da comanda pode ter no máximo {Limits.CustomerName} caracteres.");

    public static IRuleBuilderOptions<T, Guid> TableChosen<T>(this IRuleBuilder<T, Guid> rule) =>
        rule.NotEqual(Guid.Empty).WithMessage("Escolha uma mesa.");
}
