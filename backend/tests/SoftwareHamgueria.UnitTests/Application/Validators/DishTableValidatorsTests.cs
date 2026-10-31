using SoftwareHamgueria.Application.DTOs.Dish;
using SoftwareHamgueria.Application.DTOs.Table;
using SoftwareHamgueria.Application.Validators.Dish;
using SoftwareHamgueria.Application.Validators.Table;
using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.UnitTests.Application.Validators;

public class DishTableValidatorsTests
{
    private static Dictionary<string, string> Errors(CreateDishDto dto) =>
        new CreateDishDtoValidator().Validate(dto).Errors.ToDictionary(e => e.PropertyName, e => e.ErrorMessage);

    private static CreateDishDto Dish(string name = "Burger", string description = "ok", double price = 10,
        Category category = Category.Hamburgers) => new(name, description, price, category);

    [Fact(DisplayName = "003/CA-07: prato com dados válidos passa")]
    public void Dish_Valid_Passes()
    {
        Assert.True(new CreateDishDtoValidator().Validate(Dish(new string('a', 200), new string('d', 1000))).IsValid);
    }

    [Theory(DisplayName = "003/CA-07: prato com campo inválido é recusado indicando o campo")]
    [InlineData("name-empty", "Name", "Informe o nome do prato.")]
    [InlineData("name-201", "Name", "O nome do prato pode ter no máximo 200 caracteres.")]
    [InlineData("description-1001", "Description", "A descrição do prato pode ter no máximo 1000 caracteres.")]
    [InlineData("price-0", "Price", "O preço do prato precisa ser maior que zero.")]
    [InlineData("price-negative", "Price", "O preço do prato precisa ser maior que zero.")]
    [InlineData("category", "Category", "Escolha uma categoria do cardápio.")]
    public void Dish_InvalidField_IsRejected(string scenario, string field, string message)
    {
        var dto = scenario switch
        {
            "name-empty" => Dish(name: "  "),
            "name-201" => Dish(name: new string('a', 201)),
            "description-1001" => Dish(description: new string('d', 1001)),
            "price-0" => Dish(price: 0),
            "price-negative" => Dish(price: -5),
            _ => Dish(category: (Category)99),
        };

        Assert.Equal(message, Errors(dto)[field]);
    }

    [Fact(DisplayName = "003/CA-07: alterar prato segue as mesmas regras")]
    public void UpdateDish_Invalid_IsRejected()
    {
        var result = new UpdateDishDtoValidator().Validate(new UpdateDishDto("", "ok", 0, Category.Drinks));

        Assert.Equal(["Name", "Price"], result.Errors.Select(e => e.PropertyName).Order());
    }

    [Theory(DisplayName = "003/CA-08: mesa com nome vazio ou acima de 100 caracteres é recusada")]
    [InlineData(0, "Informe o nome da mesa.")]
    [InlineData(101, "O nome da mesa pode ter no máximo 100 caracteres.")]
    public void Table_InvalidName_IsRejected(int length, string message)
    {
        var name = new string('m', length);

        Assert.Equal(message, Assert.Single(new CreateTableDtoValidator().Validate(new CreateTableDto(name)).Errors).ErrorMessage);
        Assert.Equal(message, Assert.Single(new UpdateTableDtoValidator().Validate(new UpdateTableDto(name)).Errors).ErrorMessage);
    }

    [Fact(DisplayName = "003/CA-08: mesa com 100 caracteres é aceita")]
    public void Table_At100_IsValid()
    {
        Assert.True(new CreateTableDtoValidator().Validate(new CreateTableDto(new string('m', 100))).IsValid);
    }
}
