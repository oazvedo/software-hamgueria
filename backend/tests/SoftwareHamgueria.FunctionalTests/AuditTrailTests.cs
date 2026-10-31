using System.Net;
using System.Net.Http.Json;
using SoftwareHamgueria.Application.DTOs.Audit;
using SoftwareHamgueria.Application.DTOs.Order;
using SoftwareHamgueria.Domain.Entities;
using SoftwareHamgueria.FunctionalTests.Infrastructure;

namespace SoftwareHamgueria.FunctionalTests;

public class AuditTrailTests(HamgueriaApiFactory factory)
{
    private const string Screen = "http://localhost:5173/menu";

    private async Task<OrderResponseDto> CreateOrderAs(ApiClient api, string author, string customer)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/order")
        {
            Content = JsonContent.Create(await api.BuildOrderAsync(customer), options: api.Json),
        };
        request.Headers.Add("X-Author-Name", Uri.EscapeDataString(author));
        request.Headers.Add("X-Frontend-Url", Screen);
        var response = await api.Http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await api.ReadAsync<OrderResponseDto>(response))!;
    }

    private async Task<AuditPageDto> AuditAsync(ApiClient admin, string table, Guid rowId)
    {
        var response = await admin.Http.GetAsync($"/api/audit?table={table}&rowId={rowId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await admin.ReadAsync<AuditPageDto>(response))!;
    }

    [Fact(DisplayName = "009/CA-01, CA-02: pedido do cliente fica na auditoria com antes/depois, autor em nome e a tela de origem")]
    public async Task CustomerOrder_IsAudited_WithAuthorAndScreen()
    {
        var api = factory.CreateApiClient();
        var order = await CreateOrderAs(api, "João Ção", ApiClient.UniqueCustomer("Audit"));
        var admin = factory.CreateApiClient();
        await admin.LoginAsAdminAsync();

        var entry = Assert.Single((await AuditAsync(admin, "Orders", order.Id)).Items);

        Assert.Equal("INSERT", entry.Operation);
        Assert.Equal("João Ção", entry.Author);
        Assert.Equal(Screen, entry.FrontendUrl);
        Assert.False(string.IsNullOrEmpty(entry.RequestId));
        Assert.Null(entry.OldValues);
        Assert.Contains(order.CustomerName, entry.NewValues);
        var itemRows = (await admin.Http.GetFromJsonAsync<AuditPageDto>($"/api/audit?table=OrderItems&pageSize=200", admin.Json))!;
        Assert.Contains(itemRows.Items, i => i.Operation == "INSERT" && i.NewValues!.Contains(order.Id.ToString()));
    }

    [Fact(DisplayName = "009/CA-03: mudança da cozinha grava o usuário logado e só as colunas que mudaram")]
    public async Task KitchenStatusChange_IsAudited_WithChangedFields()
    {
        var api = factory.CreateApiClient();
        var order = await api.CreateOrderAsync(ApiClient.UniqueCustomer("Audit"));
        var admin = factory.CreateApiClient();
        await admin.LoginAsAdminAsync();

        (await admin.SetStatusAsync(order.Id, OrderStatus.Done)).EnsureSuccessStatusCode();

        var update = (await AuditAsync(admin, "Orders", order.Id)).Items.First(e => e.Operation == "UPDATE");
        Assert.Equal(ApiClient.AdminUsername, update.Author);
        Assert.Equal(["Status"], update.ChangedFields);
        Assert.Contains("\"Status\"", update.OldValues);
    }

    [Fact(DisplayName = "009/CA-04: alterar os itens grava a remoção do item antigo e a inclusão do novo")]
    public async Task UpdatingItems_RecordsDeleteAndInsert()
    {
        var api = factory.CreateApiClient();
        var order = await api.CreateOrderAsync(ApiClient.UniqueCustomer("Audit"));
        var other = (await api.GetDishesAsync()).First(d => d.IsActive && d.Id != order.Items[0].DishId);

        (await api.UpdateOrderAsync(order.Id, new UpdateOrderDto([new OrderItemDto(other.Id, 2)]))).EnsureSuccessStatusCode();

        var admin = factory.CreateApiClient();
        await admin.LoginAsAdminAsync();
        var items = (await admin.Http.GetFromJsonAsync<AuditPageDto>("/api/audit?table=OrderItems&pageSize=200", admin.Json))!
            .Items.Where(i => (i.OldValues ?? i.NewValues)!.Contains(order.Id.ToString())).ToList();
        Assert.Contains(items, i => i.Operation == "DELETE" && i.OldValues!.Contains(order.Items[0].DishId.ToString()));
        Assert.Contains(items, i => i.Operation == "INSERT" && i.NewValues!.Contains(other.Id.ToString()));
        Assert.All(items, i => Assert.Equal("anônimo", i.Author));
    }

    [Fact(DisplayName = "009/CA-05 (falha): sem login da cozinha, a auditoria é negada")]
    public async Task Audit_WithoutToken_Returns401()
    {
        var response = await factory.CreateApiClient().Http.GetAsync("/api/audit");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "009/CA-06 (falha): filtro por tabela fora da auditoria é recusado com o motivo")]
    public async Task Audit_UnknownTable_Returns400()
    {
        var admin = factory.CreateApiClient();
        await admin.LoginAsAdminAsync();

        var response = await admin.Http.GetAsync("/api/audit?table=pg_shadow");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("table", (await admin.ReadProblemAsync(response)).Errors!.Keys, StringComparer.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "009/RN-05: a semeadura do banco é gravada como 'sistema'")]
    public async Task Seed_IsAuthoredBySystem()
    {
        var admin = factory.CreateApiClient();
        await admin.LoginAsAdminAsync();

        // Other tests add dishes in parallel: look for a dish only the seed creates.
        var page = (await admin.Http.GetFromJsonAsync<AuditPageDto>("/api/audit?table=Dishes&pageSize=200", admin.Json))!;
        var seeded = page.Items.Single(i => i.Operation == "INSERT" && i.NewValues!.Contains("\"Burger Monolito\""));

        Assert.Equal("sistema", seeded.Author);
        Assert.Null(seeded.FrontendUrl);
    }
}
