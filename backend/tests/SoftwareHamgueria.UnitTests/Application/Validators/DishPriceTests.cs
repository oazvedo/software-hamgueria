using SoftwareHamgueria.Application.DTOs.Dish;
using SoftwareHamgueria.Application.Validators.Dish;
using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.UnitTests.Application.Validators;

public class DishPriceTests
{
    private static IEnumerable<string> Errors(decimal price) =>
        new CreateDishDtoValidator().Validate(new CreateDishDto("Burger", "ok", price, Category.Hamburgers))
            .Errors.Select(e => e.ErrorMessage);

    [Theory(DisplayName = "016/CA-01: preço com até 2 casas é aceito, exatamente como foi digitado")]
    [InlineData("0.01")]
    [InlineData("19.99")]
    [InlineData("34.90")]
    [InlineData("99999999.99")]
    public void TwoPlaces_AreAccepted(string price) => Assert.Empty(Errors(decimal.Parse(price, System.Globalization.CultureInfo.InvariantCulture)));

    [Theory(DisplayName = "016/CA-02 (falha): preço com mais de 2 casas é recusado em vez de arredondado")]
    [InlineData("19.999")]
    [InlineData("0.001")]
    public void MoreThanTwoPlaces_AreRejected(string price) =>
        Assert.Contains("O preço do prato vai até 99.999.999,99, com no máximo 2 casas decimais.",
            Errors(decimal.Parse(price, System.Globalization.CultureInfo.InvariantCulture)));

    [Fact(DisplayName = "016/CA-03: somar preços não perde centavo (0,10 + 0,20 = 0,30)")]
    public void Decimal_SumsExactly() => Assert.Equal(0.30m, 0.10m + 0.20m);
}
