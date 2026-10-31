using System.Net;
using SoftwareHamgueria.Application.DTOs.Order;
using SoftwareHamgueria.Domain.Entities;
using SoftwareHamgueria.FunctionalTests.Infrastructure;

namespace SoftwareHamgueria.FunctionalTests;

public class OrderOwnershipTests(HamgueriaApiFactory factory)
{
    [Fact(DisplayName = "012/CA-01: criar o pedido devolve o token de dono uma vez; consultar não devolve")]
    public async Task Create_ReturnsOwnerToken_OnlyOnce()
    {
        var api = factory.CreateApiClient();
        var order = await api.CreateOrderAsync(ApiClient.UniqueCustomer("Dono"));

        Assert.False(string.IsNullOrEmpty(order.OwnerToken));
        Assert.Null((await api.GetOrderAsync(order.Id)).OwnerToken);
    }

    [Fact(DisplayName = "012/CA-02: com o token de dono, o cliente cancela e altera o próprio pedido")]
    public async Task Owner_CanUpdateAndCancel()
    {
        var api = factory.CreateApiClient();
        var order = await api.CreateOrderAsync(ApiClient.UniqueCustomer("Dono"));

        Assert.Equal(HttpStatusCode.OK, (await api.UpdateOrderAsync(order.Id, new UpdateOrderDto([new OrderItemDto(order.Items[0].DishId, 2)]))).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await api.CancelAsync(order.Id)).StatusCode);
    }

    [Theory(DisplayName = "012/CA-03 (falha): sem token, ou com o token de outro pedido, alterar e cancelar são negados")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Stranger_IsForbidden(bool withAnotherOrdersToken)
    {
        var owner = factory.CreateApiClient();
        var order = await owner.CreateOrderAsync(ApiClient.UniqueCustomer("Dono"));
        var stranger = factory.CreateApiClient();
        var token = "";
        if (withAnotherOrdersToken)
        {
            var other = await stranger.CreateOrderAsync(ApiClient.UniqueCustomer("Outro"));
            token = stranger.OwnerTokenOf(other.Id)!;
        }

        var cancel = await stranger.CancelAsync(order.Id, token);
        var update = await stranger.UpdateOrderAsync(order.Id, new UpdateOrderDto([new OrderItemDto(order.Items[0].DishId, 3)]), token);

        Assert.Equal(HttpStatusCode.Forbidden, cancel.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, update.StatusCode);
        Assert.Contains("Só quem fez o pedido", (await stranger.ReadProblemAsync(cancel)).Detail);
        var after = await owner.GetOrderAsync(order.Id);
        Assert.Equal(OrderStatus.Created, after.Status);
        Assert.Equal(order.Items[0].Quantity, after.Items[0].Quantity);
    }

    [Fact(DisplayName = "012/CA-02: a cozinha cancela qualquer pedido, sem token")]
    public async Task Kitchen_CancelsWithoutToken()
    {
        var order = await factory.CreateApiClient().CreateOrderAsync(ApiClient.UniqueCustomer("Dono"));
        var kitchen = factory.CreateApiClient();
        await kitchen.LoginAsAdminAsync();

        Assert.Equal(HttpStatusCode.NoContent, (await kitchen.CancelAsync(order.Id, "")).StatusCode);
    }

    [Fact(DisplayName = "012/CA-05 (falha): pedido servido não pode ser cancelado, alterado nem voltar de status")]
    public async Task ServedOrder_IsFinal()
    {
        var api = factory.CreateApiClient();
        var order = await api.CreateOrderAsync(ApiClient.UniqueCustomer("Dono"));
        var kitchen = factory.CreateApiClient();
        await kitchen.LoginAsAdminAsync();
        (await kitchen.SetStatusAsync(order.Id, OrderStatus.Done)).EnsureSuccessStatusCode();

        var cancel = await api.CancelAsync(order.Id);
        var update = await api.UpdateOrderAsync(order.Id, new UpdateOrderDto([new OrderItemDto(order.Items[0].DishId, 1)]));
        var back = await kitchen.SetStatusAsync(order.Id, OrderStatus.Created);

        Assert.Equal(HttpStatusCode.BadRequest, cancel.StatusCode);
        Assert.Equal("Um pedido já servido não pode ser cancelado.", (await api.ReadProblemAsync(cancel)).Detail);
        Assert.Equal(HttpStatusCode.BadRequest, update.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, back.StatusCode);
        Assert.Equal("Um pedido já servido não muda mais de status.", (await kitchen.ReadProblemAsync(back)).Detail);
        Assert.Equal(OrderStatus.Done, (await api.GetOrderAsync(order.Id)).Status);
    }

    [Fact(DisplayName = "012/CA-05 (falha): pedido cancelado não volta para produção")]
    public async Task CancelledOrder_IsFinal()
    {
        var api = factory.CreateApiClient();
        var order = await api.CreateOrderAsync(ApiClient.UniqueCustomer("Dono"));
        (await api.CancelAsync(order.Id)).EnsureSuccessStatusCode();
        var kitchen = factory.CreateApiClient();
        await kitchen.LoginAsAdminAsync();

        var response = await kitchen.SetStatusAsync(order.Id, OrderStatus.Done);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(OrderStatus.Cancelled, (await api.GetOrderAsync(order.Id)).Status);
    }
}
