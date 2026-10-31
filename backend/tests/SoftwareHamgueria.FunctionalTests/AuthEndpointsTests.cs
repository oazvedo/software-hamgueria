using System.Net;
using System.Net.Http.Json;
using SoftwareHamgueria.Application.DTOs.Auth;
using SoftwareHamgueria.Application.DTOs.Order;
using SoftwareHamgueria.Domain.Entities;
using SoftwareHamgueria.FunctionalTests.Infrastructure;

namespace SoftwareHamgueria.FunctionalTests;

public class AuthEndpointsTests(HamgueriaApiFactory factory)
{
    [Fact(DisplayName = "001/CA-09: sem login, a lista completa de pedidos da cozinha é negada")]
    public async Task GetAllOrders_WithoutToken_Returns401()
    {
        var response = await factory.CreateApiClient().Http.GetAsync("/api/order");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "001/CA-10: credenciais erradas da cozinha são negadas")]
    public async Task Login_WrongPassword_Returns401()
    {
        var response = await factory.CreateApiClient().LoginAsync(ApiClient.AdminUsername, "senha-errada");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "001/CA-10: credenciais certas da cozinha devolvem um token válido")]
    public async Task Login_RightPassword_ReturnsToken()
    {
        var api = factory.CreateApiClient();

        var response = await api.LoginAsync(ApiClient.AdminUsername, ApiClient.AdminPassword);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var token = (await api.ReadAsync<TokenResponseDto>(response))!;
        Assert.False(string.IsNullOrWhiteSpace(token.Token));
        Assert.True(token.ExpiresAt > DateTime.UtcNow);
    }

    [Fact(DisplayName = "001/CA-11: a cozinha autenticada vê o pedido e o finaliza, e o cliente vê o novo estado")]
    public async Task Kitchen_FinishesOrder_CustomerSeesDone()
    {
        var customer = factory.CreateApiClient();
        var order = await customer.CreateOrderAsync(ApiClient.UniqueCustomer());
        var kitchen = factory.CreateApiClient();
        await kitchen.LoginAsAdminAsync();

        var all = await kitchen.Http.GetFromJsonAsync<List<OrderResponseDto>>("/api/order", kitchen.Json);
        var response = await kitchen.SetStatusAsync(order.Id, OrderStatus.Done);

        Assert.Contains(all!, o => o.Id == order.Id);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(OrderStatus.Done, (await customer.GetOrderAsync(order.Id)).Status);
    }

    [Fact(DisplayName = "001/CA-11: cancelar um pedido muda o estado para cancelado e gera o evento")]
    public async Task Cancel_Order_CustomerSeesCancelled()
    {
        var api = factory.CreateApiClient();
        var order = await api.CreateOrderAsync(ApiClient.UniqueCustomer());

        var response = await api.CancelAsync(order.Id);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(OrderStatus.Cancelled, (await api.GetOrderAsync(order.Id)).Status);
        Assert.True(factory.Publisher.WasPublished("order.cancelled", order.Id.ToString()));
    }

    [Fact(DisplayName = "001/CA-09: sem login, mudar o status do pedido é negado")]
    public async Task SetStatus_WithoutToken_Returns401()
    {
        var api = factory.CreateApiClient();
        var order = await api.CreateOrderAsync(ApiClient.UniqueCustomer());

        var response = await api.SetStatusAsync(order.Id, OrderStatus.Done);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(OrderStatus.Created, (await api.GetOrderAsync(order.Id)).Status);
    }
}
