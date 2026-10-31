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
        Assert.Contains(dish.Name, await response.Content.ReadAsStringAsync());
    }

    [Fact(DisplayName = "001/CA-05: pedido com prato inexistente é recusado")]
    public async Task Post_UnknownDish_Returns404()
    {
        var response = await _api.PostOrderAsync(new CreateOrderDto(
            await _api.FirstTableIdAsync(), ApiClient.UniqueCustomer(), [new OrderItemDto(Guid.NewGuid(), 1)]));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "001/RN-03: pedido para mesa inexistente é recusado")]
    public async Task Post_UnknownTable_Returns404()
    {
        var response = await _api.PostOrderAsync(await _api.BuildOrderAsync(ApiClient.UniqueCustomer(), Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
}
