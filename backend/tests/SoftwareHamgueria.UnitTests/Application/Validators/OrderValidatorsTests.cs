using FluentValidation.Results;
using SoftwareHamgueria.Application.DTOs.Order;
using SoftwareHamgueria.Application.Validators.Order;

namespace SoftwareHamgueria.UnitTests.Application.Validators;

public class OrderValidatorsTests
{
    private static readonly Guid Table = Guid.NewGuid();
    private static readonly Guid Dish = Guid.NewGuid();

    private static CreateOrderDto Order(string customer = "Ana", params OrderItemDto[] items) =>
        new(Table, customer, items.Length == 0 ? [new OrderItemDto(Dish, 1)] : [.. items]);

    private static ValidationResult Validate(CreateOrderDto dto) => new CreateOrderDtoValidator().Validate(dto);

    private static Dictionary<string, string> Errors(ValidationResult r) =>
        r.Errors.ToDictionary(e => e.PropertyName, e => e.ErrorMessage);

    [Fact(DisplayName = "003/CA-01: pedido com 20 unidades e observação de 200 caracteres é válido")]
    public void Create_AtTheLimits_IsValid()
    {
        var result = Validate(Order("Ana", new OrderItemDto(Dish, 20, new string('x', 200))));

        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "003/CA-02: pedido sem nenhum item é recusado")]
    public void Create_WithoutItems_IsRejected()
    {
        var result = Validate(new CreateOrderDto(Table, "Ana", []));

        Assert.Equal("O pedido precisa de pelo menos um item.", Errors(result)["Items"]);
    }

    [Fact(DisplayName = "003/CA-02: pedido sem a lista de itens é recusado")]
    public void Create_NullItems_IsRejected()
    {
        var result = Validate(new CreateOrderDto(Table, "Ana", null!));

        Assert.Contains("Items", Errors(result).Keys);
    }

    [Theory(DisplayName = "003/CA-03: quantidade fora de 1 a 20 é recusada indicando o item")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(21)]
    public void Create_QuantityOutOfRange_IsRejected(int quantity)
    {
        var result = Validate(Order("Ana", new OrderItemDto(Guid.NewGuid(), 1), new OrderItemDto(Dish, quantity)));

        Assert.Equal("A quantidade do item 2 vai de 1 a 20.", Errors(result)["Items[1].Quantity"]);
    }

    [Fact(DisplayName = "003/CA-03: o mesmo prato repetido não passa de 20 somando as repetições")]
    public void Create_RepeatedDishOver20_IsRejected()
    {
        var result = Validate(Order("Ana", new OrderItemDto(Dish, 15), new OrderItemDto(Dish, 6)));

        Assert.Equal(["Items[1].Quantity"], Errors(result).Keys);
    }

    [Fact(DisplayName = "003/CA-03: alterar o pedido segue as mesmas regras de item")]
    public void Update_QuantityOutOfRange_IsRejected()
    {
        var result = new UpdateOrderDtoValidator().Validate(new UpdateOrderDto([new OrderItemDto(Dish, 0)]));

        Assert.Equal("A quantidade do item 1 vai de 1 a 20.", Errors(result)["Items[0].Quantity"]);
    }

    [Fact(DisplayName = "003/CA-03: alterar o pedido sem itens é recusado")]
    public void Update_WithoutItems_IsRejected()
    {
        var result = new UpdateOrderDtoValidator().Validate(new UpdateOrderDto([]));

        Assert.Contains("Items", Errors(result).Keys);
    }

    [Fact(DisplayName = "003/CA-04: observação com 201 caracteres é recusada com o limite")]
    public void Create_NotesTooLong_IsRejected()
    {
        var result = Validate(Order("Ana", new OrderItemDto(Dish, 1, new string('x', 201))));

        Assert.Equal("A observação do item 1 pode ter no máximo 200 caracteres.", Errors(result)["Items[0].Notes"]);
    }

    [Theory(DisplayName = "003/CA-05: nome de comanda vazio, só com espaços ou longo demais é recusado")]
    [InlineData("", "Informe o nome da comanda.")]
    [InlineData("   ", "Informe o nome da comanda.")]
    [InlineData(null, "Informe o nome da comanda.")]
    [InlineData("61", "O nome da comanda pode ter no máximo 60 caracteres.")]
    public void Create_InvalidCustomerName_IsRejected(string? customer, string message)
    {
        var name = customer == "61" ? new string('a', 61) : customer;

        var result = Validate(Order(name!));

        Assert.Equal(message, Errors(result)["CustomerName"]);
    }

    [Fact(DisplayName = "003/CA-05: nome de comanda com 60 caracteres, mesmo com espaços nas pontas, é aceito")]
    public void Create_CustomerNameAt60_IsValid()
    {
        Assert.True(Validate(Order($"  {new string('a', 60)}  ")).IsValid);
    }

    [Theory(DisplayName = "003/CA-05: fechar comanda e consultar comanda checam o nome")]
    [InlineData("")]
    [InlineData("61")]
    public void CloseAndQuery_InvalidCustomerName_IsRejected(string customer)
    {
        var name = customer == "61" ? new string('a', 61) : customer;

        Assert.Contains("CustomerName", Errors(new CloseComandaDtoValidator().Validate(new CloseComandaDto(Table, name))).Keys);
        Assert.Contains("Customer", Errors(new ComandaQueryValidator().Validate(new ComandaQuery(Table, name))).Keys);
    }

    [Fact(DisplayName = "003/CA-05: fechar comanda com nome válido é aceito")]
    public void Close_ValidName_IsValid()
    {
        Assert.True(new CloseComandaDtoValidator().Validate(new CloseComandaDto(Table, "Ana")).IsValid);
    }

    [Fact(DisplayName = "003/CA-06: vários problemas vêm de uma vez")]
    public void Create_SeveralProblems_AreAllReported()
    {
        var result = Validate(Order("", new OrderItemDto(Dish, 0)));

        Assert.Equal(["CustomerName", "Items[0].Quantity"], Errors(result).Keys.Order());
    }

    [Fact(DisplayName = "003/RN-01: pedido sem mesa é recusado")]
    public void Create_WithoutTable_IsRejected()
    {
        var result = Validate(new CreateOrderDto(Guid.Empty, "Ana", [new OrderItemDto(Dish, 1)]));

        Assert.Equal("Escolha uma mesa.", Errors(result)["TableId"]);
    }
}
