using FluentValidation;
using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.Application.Validators.Dish;

/// <summary>Dish fields shared by create and update (spec 003, RN-03).</summary>
public static class DishRuleExtensions
{
    public static IRuleBuilderOptions<T, string> DishName<T>(this IRuleBuilder<T, string> rule) =>
        rule.RequiredTrimmed(Limits.DishName,
            "Informe o nome do prato.",
            $"O nome do prato pode ter no máximo {Limits.DishName} caracteres.");

    public static IRuleBuilderOptions<T, string> DishDescription<T>(this IRuleBuilder<T, string> rule) =>
        rule.Must(d => d is null || d.Length <= Limits.DishDescription)
            .WithMessage($"A descrição do prato pode ter no máximo {Limits.DishDescription} caracteres.");

    public static IRuleBuilderOptions<T, double> DishPrice<T>(this IRuleBuilder<T, double> rule) =>
        rule.GreaterThan(0).WithMessage("O preço do prato precisa ser maior que zero.");

    public static IRuleBuilderOptions<T, Category> DishCategory<T>(this IRuleBuilder<T, Category> rule) =>
        rule.IsInEnum().WithMessage("Escolha uma categoria do cardápio.");
}
