using System.Net;
using SoftwareHamgueria.FunctionalTests.Infrastructure;

namespace SoftwareHamgueria.FunctionalTests;

public class ComandaEndpointsTests(HamgueriaApiFactory factory)
{
    private readonly ApiClient _api = factory.CreateApiClient();

    [Fact(DisplayName = "001/CA-07: a comanda reúne todos os pedidos do mesmo nome na mesa, sem diferenciar maiúsculas")]
    public async Task GetComanda_SameNameAnyCase_ReturnsAllOrders()
    {
        var customer = ApiClient.UniqueCustomer("ana");
        var first = await _api.CreateOrderAsync(customer);
        var second = await _api.CreateOrderAsync(customer.ToUpperInvariant());

        var comanda = await _api.GetComandaAsync(first.TableId, customer);

        Assert.Equal([first.Id, second.Id], comanda.Select(o => o.Id));
    }

    [Fact(DisplayName = "001/CA-07: a comanda de outra pessoa da mesa não aparece na minha")]
    public async Task GetComanda_OtherCustomer_IsNotIncluded()
    {
        var mine = await _api.CreateOrderAsync(ApiClient.UniqueCustomer());
        await _api.CreateOrderAsync(ApiClient.UniqueCustomer());

        var comanda = await _api.GetComandaAsync(mine.TableId, mine.CustomerName);

        Assert.Equal(mine.Id, Assert.Single(comanda).Id);
    }

    [Fact(DisplayName = "001/CA-07: a mesa mostra os pedidos de todas as comandas dela")]
    public async Task GetTableOrders_ReturnsEveryComandaOfTheTable()
    {
        var ana = await _api.CreateOrderAsync(ApiClient.UniqueCustomer());
        var bia = await _api.CreateOrderAsync(ApiClient.UniqueCustomer());

        var orders = await _api.GetTableOrdersAsync(ana.TableId);

        Assert.Contains(orders, o => o.Id == ana.Id);
        Assert.Contains(orders, o => o.Id == bia.Id);
    }

    [Fact(DisplayName = "001/CA-08: fechar a comanda marca os pedidos dela como fechados")]
    public async Task Close_OpenComanda_MarksOrdersClosed()
    {
        var order = await _api.CreateOrderAsync(ApiClient.UniqueCustomer());

        var response = await _api.CloseComandaAsync(order.TableId, order.CustomerName);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var comanda = await _api.GetComandaAsync(order.TableId, order.CustomerName);
        Assert.True(Assert.Single(comanda).ComandaClosed);
    }

    [Fact(DisplayName = "001/CA-08: pedido novo com o mesmo nome depois do fechamento abre comanda nova")]
    public async Task Order_AfterClosing_OpensNewComanda()
    {
        var old = await _api.CreateOrderAsync(ApiClient.UniqueCustomer());
        (await _api.CloseComandaAsync(old.TableId, old.CustomerName)).EnsureSuccessStatusCode();

        var fresh = await _api.CreateOrderAsync(old.CustomerName, old.TableId);

        var comanda = await _api.GetComandaAsync(old.TableId, old.CustomerName);
        Assert.True(comanda.Single(o => o.Id == old.Id).ComandaClosed);
        Assert.False(comanda.Single(o => o.Id == fresh.Id).ComandaClosed);
    }

    [Fact(DisplayName = "001/CA-08: fechar comanda sem nome é recusado")]
    public async Task Close_WithoutCustomerName_Returns400()
    {
        var response = await _api.CloseComandaAsync(await _api.FirstTableIdAsync(), " ");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(["Informe o nome da comanda."], (await _api.ReadProblemAsync(response)).Errors!["customerName"]);
    }

    [Fact(DisplayName = "003/CA-05: fechar comanda com nome de 61 caracteres é recusado")]
    public async Task Close_NameTooLong_Returns400()
    {
        var response = await _api.CloseComandaAsync(await _api.FirstTableIdAsync(), new string('a', 61));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("customerName", (await _api.ReadProblemAsync(response)).Errors!.Keys);
    }

    [Fact(DisplayName = "003/CA-05: consultar comanda sem nome é recusado no formato padrão")]
    public async Task GetComanda_WithoutCustomer_Returns400()
    {
        var response = await _api.Http.GetAsync($"/api/order/comanda?tableId={await _api.FirstTableIdAsync()}&customer=");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("customer", (await _api.ReadProblemAsync(response)).Errors!.Keys);
    }
}
