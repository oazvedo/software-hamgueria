using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SoftwareHamgueria.Application.DTOs.Dish;
using SoftwareHamgueria.Domain.Entities;
using SoftwareHamgueria.FunctionalTests.Infrastructure;

namespace SoftwareHamgueria.FunctionalTests;

public class DishPriceEndpointsTests(HamgueriaApiFactory factory)
{
    [Fact(DisplayName = "016/CA-01: o preço gravado volta exatamente igual, sem resto de ponto flutuante")]
    public async Task Price_RoundTripsExactly()
    {
        var admin = factory.CreateApiClient();
        await admin.LoginAsAdminAsync();

        var created = await admin.Http.PostAsJsonAsync("/api/dish",
            new CreateDishDto($"Preço {Guid.NewGuid():N}"[..30], "teste", 19.99m, Category.Drinks), admin.Json);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var id = (await admin.ReadAsync<DishResponseDto>(created))!.Id;

        var raw = await admin.Http.GetStringAsync($"/api/dish/{id}");
        Assert.Equal(19.99m, JsonDocument.Parse(raw).RootElement.GetProperty("price").GetDecimal());
        Assert.Contains("\"price\":19.99", raw);
    }

    [Fact(DisplayName = "016/CA-02 (falha): preço com 3 casas é recusado pela API")]
    public async Task Price_WithThreePlaces_Returns400()
    {
        var admin = factory.CreateApiClient();
        await admin.LoginAsAdminAsync();

        var response = await admin.Http.PostAsJsonAsync("/api/dish",
            new CreateDishDto("Prato com centavo quebrado", "teste", 19.999m, Category.Drinks), admin.Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("price", (await admin.ReadProblemAsync(response)).Errors!.Keys, StringComparer.OrdinalIgnoreCase);
    }
}
