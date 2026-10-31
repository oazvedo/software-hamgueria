using System.Net;
using System.Net.Http.Json;
using SoftwareHamgueria.Application.DTOs.Dish;
using SoftwareHamgueria.Application.DTOs.Order;
using SoftwareHamgueria.Domain.Entities;
using SoftwareHamgueria.FunctionalTests.Infrastructure;

namespace SoftwareHamgueria.FunctionalTests;

public class OrderEndpointsTests(HamgueriaApiFactory factory)
{
    private readonly ApiClient _api = factory.CreateApiClient();

    [Fact(DisplayName = "001/CA-03: pedido com mesa, itens e nome é criado como recebido e gera o evento order.created")]
    public async Task Post_ValidOrder_Returns201AsCreated()
    {
        var customer = ApiClient.UniqueCustomer();

        var response = await _api.PostOrderAsync(await _api.BuildOrderAsync(customer, quantity: 2, notes: "sem cebola"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var order = (await _api.ReadAsync<OrderResponseDto>(response))!;
        Assert.Equal($"/api/Order/{order.Id}", response.Headers.Location?.AbsolutePath, ignoreCase: true);
        Assert.Equal(OrderStatus.Created, order.Status);
        Assert.Equal(customer, order.CustomerName);
        var item = Assert.Single(order.Items);
        Assert.Equal((2, "sem cebola"), (item.Quantity, item.Notes));
        Assert.True(factory.Publisher.WasPublished("order.created", order.Id.ToString()));
    }

    [Theory(DisplayName = "001/CA-04: pedido sem nome de comanda é recusado")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Post_WithoutCustomerName_Returns400(string customer)
    {
        var response = await _api.PostOrderAsync(await _api.BuildOrderAsync(customer));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await _api.ReadProblemAsync(response);
        Assert.Equal(ProblemBody.Validation, problem.Type);
        Assert.Equal(["Informe o nome da comanda."], problem.Errors!["customerName"]);
    }

    [Fact(DisplayName = "001/CA-05: pedido com prato inativo é recusado com o nome do prato")]
    public async Task Post_InactiveDish_Returns400WithDishName()
    {
        // Its own dish, so disabling it never affects the seeded menu other tests use.
        var admin = factory.CreateApiClient();
        await admin.LoginAsAdminAsync();
        var created = await admin.Http.PostAsJsonAsync("/api/dish",
            new CreateDishDto($"Inativo {Guid.NewGuid():N}", "teste", 10, Category.Starters), admin.Json);
        var dish = (await admin.ReadAsync<DishResponseDto>(created))!;
        (await admin.Http.PatchAsync($"/api/dish/{dish.Id}/disable", null)).EnsureSuccessStatusCode();

        var response = await _api.PostOrderAsync(new CreateOrderDto(
            await _api.FirstTableIdAsync(), ApiClient.UniqueCustomer(), [new OrderItemDto(dish.Id, 1)]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await _api.ReadProblemAsync(response);
        Assert.Equal(ProblemBody.BusinessRule, problem.Type);
        Assert.Equal($"O prato {dish.Name} não está disponível.", problem.Detail);
    }

    [Fact(DisplayName = "001/CA-05: pedido com prato inexistente é recusado")]
    public async Task Post_UnknownDish_Returns404()
    {
        var response = await _api.PostOrderAsync(new CreateOrderDto(
            await _api.FirstTableIdAsync(), ApiClient.UniqueCustomer(), [new OrderItemDto(Guid.NewGuid(), 1)]));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("O prato informado não existe.", (await _api.ReadProblemAsync(response)).Detail);
    }

    [Fact(DisplayName = "001/RN-03: pedido para mesa inexistente é recusado")]
    public async Task Post_UnknownTable_Returns404()
    {
        var response = await _api.PostOrderAsync(await _api.BuildOrderAsync(ApiClient.UniqueCustomer(), Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("A mesa informada não existe.", (await _api.ReadProblemAsync(response)).Detail);
    }

    [Fact(DisplayName = "001/CA-06: o pedido criado pode ser consultado pelo id")]
    public async Task Get_ExistingOrder_Returns200()
    {
        var order = await _api.CreateOrderAsync(ApiClient.UniqueCustomer());

        var fetched = await _api.GetOrderAsync(order.Id);

        Assert.Equal(order.Id, fetched.Id);
        Assert.Equal(OrderStatus.Created, fetched.Status);
    }

    [Fact(DisplayName = "001/CA-06: consultar pedido inexistente devolve não encontrado")]
    public async Task Get_UnknownOrder_Returns404()
    {
        var response = await _api.GetOrderResponseAsync(Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---- spec 003: validation

    private async Task<(HttpResponseMessage Response, string Customer)> PostAsync(int quantity = 1, string? notes = null,
        string? customer = null)
    {
        customer ??= ApiClient.UniqueCustomer();
        return (await _api.PostOrderAsync(await _api.BuildOrderAsync(customer, quantity: quantity, notes: notes)), customer);
    }

    private async Task AssertNothingSavedAsync(string customer) =>
        Assert.Empty(await _api.GetComandaAsync(await _api.FirstTableIdAsync(), customer));

    [Fact(DisplayName = "003/CA-01: pedido no limite (20 unidades, observação de 200) é criado")]
    public async Task Post_AtTheLimits_Returns201()
    {
        var (response, _) = await PostAsync(quantity: 20, notes: new string('x', 200));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact(DisplayName = "003/CA-02: pedido sem nenhum item é recusado")]
    public async Task Post_WithoutItems_Returns400()
    {
        var customer = ApiClient.UniqueCustomer();
        var response = await _api.PostOrderAsync(new CreateOrderDto(await _api.FirstTableIdAsync(), customer, []));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(["O pedido precisa de pelo menos um item."], (await _api.ReadProblemAsync(response)).Errors!["items"]);
        await AssertNothingSavedAsync(customer);
    }

    [Theory(DisplayName = "003/CA-03: quantidade 0, negativa ou acima de 20 é recusada e nada é gravado")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(21)]
    public async Task Post_QuantityOutOfRange_Returns400(int quantity)
    {
        var (response, customer) = await PostAsync(quantity);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(["A quantidade do item 1 vai de 1 a 20."], (await _api.ReadProblemAsync(response)).Errors!["items[0].quantity"]);
        await AssertNothingSavedAsync(customer);
    }

    [Fact(DisplayName = "005/CA-03: alterar o pedido troca os itens e publica order.updated")]
    public async Task Put_ValidItems_ReplacesItems()
    {
        var order = await _api.CreateOrderAsync(ApiClient.UniqueCustomer());
        var otherDish = (await _api.GetDishesAsync()).First(d => d.IsActive && d.Id != order.Items[0].DishId);

        var response = await _api.UpdateOrderAsync(order.Id,
            new UpdateOrderDto([new OrderItemDto(otherDish.Id, 3, "sem cebola")]));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var item = Assert.Single((await _api.GetOrderAsync(order.Id)).Items);
        Assert.Equal((otherDish.Id, 3, "sem cebola"), (item.DishId, item.Quantity, item.Notes));
        Assert.True(factory.Publisher.WasPublished("order.updated", order.Id.ToString()));
    }

    [Fact(DisplayName = "003/CA-03: alterar o pedido com quantidade 0 é recusado e o pedido não muda")]
    public async Task Put_QuantityZero_Returns400()
    {
        var order = await _api.CreateOrderAsync(ApiClient.UniqueCustomer());

        var response = await _api.Http.PutAsJsonAsync($"/api/order/{order.Id}",
            new UpdateOrderDto([new OrderItemDto(order.Items[0].DishId, 0)]), _api.Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("items[0].quantity", (await _api.ReadProblemAsync(response)).Errors!.Keys);
        Assert.Equal(1, (await _api.GetOrderAsync(order.Id)).Items[0].Quantity);
    }

    [Fact(DisplayName = "003/CA-04: observação com 201 caracteres é recusada com o limite de 200")]
    public async Task Post_NotesTooLong_Returns400()
    {
        var (response, _) = await PostAsync(notes: new string('x', 201));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(["A observação do item 1 pode ter no máximo 200 caracteres."],
            (await _api.ReadProblemAsync(response)).Errors!["items[0].notes"]);
    }

    [Fact(DisplayName = "003/CA-05: nome de comanda com 61 caracteres é recusado e com 60 é aceito")]
    public async Task Post_CustomerNameLength_Limit60()
    {
        var (tooLong, _) = await PostAsync(customer: new string('a', 61));
        var (atLimit, _) = await PostAsync(customer: $"{Guid.NewGuid():N}{new string('a', 28)}");

        Assert.Equal(HttpStatusCode.BadRequest, tooLong.StatusCode);
        Assert.Equal(["O nome da comanda pode ter no máximo 60 caracteres."],
            (await _api.ReadProblemAsync(tooLong)).Errors!["customerName"]);
        Assert.Equal(HttpStatusCode.Created, atLimit.StatusCode);
    }

    [Fact(DisplayName = "003/CA-06: sem nome e com quantidade 0, os dois problemas vêm de uma vez")]
    public async Task Post_SeveralProblems_AllReported()
    {
        var (response, _) = await PostAsync(quantity: 0, customer: "");

        Assert.Equal(["customerName", "items[0].quantity"], (await _api.ReadProblemAsync(response)).Errors!.Keys.Order());
    }
}
