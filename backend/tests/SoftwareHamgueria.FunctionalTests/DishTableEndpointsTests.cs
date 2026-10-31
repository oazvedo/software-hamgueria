using System.Net;
using System.Net.Http.Json;
using SoftwareHamgueria.Application.DTOs.Dish;
using SoftwareHamgueria.Application.DTOs.Table;
using SoftwareHamgueria.Domain.Entities;
using SoftwareHamgueria.FunctionalTests.Infrastructure;

namespace SoftwareHamgueria.FunctionalTests;

public class DishTableEndpointsTests(HamgueriaApiFactory factory)
{
    private async Task<ApiClient> AdminAsync()
    {
        var admin = factory.CreateApiClient();
        await admin.LoginAsAdminAsync();
        return admin;
    }

    [Fact(DisplayName = "003/CA-07: prato com nome vazio e preço negativo é recusado por campo e o cardápio não muda")]
    public async Task PostDish_Invalid_Returns400AndMenuUnchanged()
    {
        var admin = await AdminAsync();
        var before = (await admin.GetDishesAsync()).Count;

        var response = await admin.Http.PostAsJsonAsync("/api/dish",
            new CreateDishDto(" ", new string('d', 1001), -1, Category.Drinks), admin.Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await admin.ReadProblemAsync(response);
        Assert.Equal(ProblemBody.Validation, problem.Type);
        Assert.Equal(["description", "name", "price"], problem.Errors!.Keys.Order());
        Assert.Equal(before, (await admin.GetDishesAsync()).Count);
    }

    [Fact(DisplayName = "003/CA-07: alterar prato com preço zero ou nome de 201 caracteres é recusado e o prato não muda")]
    public async Task PutDish_Invalid_Returns400AndDishUnchanged()
    {
        var admin = await AdminAsync();
        var dish = (await admin.GetDishesAsync())[0];

        var response = await admin.Http.PutAsJsonAsync($"/api/dish/{dish.Id}",
            new UpdateDishDto(new string('a', 201), dish.Description, 0, dish.Category), admin.Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(["name", "price"], (await admin.ReadProblemAsync(response)).Errors!.Keys.Order());
        Assert.Equal(dish.Name, (await admin.GetDishesAsync()).Single(d => d.Id == dish.Id).Name);
    }

    [Fact(DisplayName = "003/CA-07: prato com categoria fora do cardápio é recusado")]
    public async Task PostDish_UnknownCategory_Returns400()
    {
        var admin = await AdminAsync();

        var response = await admin.Http.PostAsJsonAsync("/api/dish",
            new { name = "Burger", description = "x", price = 10, category = 99 }, admin.Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("category", (await admin.ReadProblemAsync(response)).Errors!.Keys);
    }

    [Fact(DisplayName = "003/CA-07: prato válido é criado com o nome aparado")]
    public async Task PostDish_Valid_Returns201()
    {
        var admin = await AdminAsync();
        var name = $"Prato {Guid.NewGuid():N}";

        var response = await admin.Http.PostAsJsonAsync("/api/dish",
            new CreateDishDto($"  {name}  ", "x", 10, Category.Starters), admin.Json);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(name, (await admin.ReadAsync<DishResponseDto>(response))!.Name);
    }

    [Fact(DisplayName = "003/CA-07: cadastrar prato sem login é negado no formato padrão")]
    public async Task PostDish_WithoutLogin_Returns401()
    {
        var api = factory.CreateApiClient();

        var response = await api.Http.PostAsJsonAsync("/api/dish", new CreateDishDto("x", "x", 1, Category.Drinks), api.Json);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(ProblemBody.Unauthorized, (await api.ReadProblemAsync(response)).Type);
    }

    [Theory(DisplayName = "003/CA-08: mesa com nome vazio ou acima de 100 caracteres é recusada e as mesas não mudam")]
    [InlineData(0)]
    [InlineData(101)]
    public async Task PostTable_Invalid_Returns400AndTablesUnchanged(int length)
    {
        var admin = await AdminAsync();
        var before = (await admin.GetTablesAsync()).Count;

        var response = await admin.Http.PostAsJsonAsync("/api/table", new CreateTableDto(new string('m', length)), admin.Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(["tableName"], (await admin.ReadProblemAsync(response)).Errors!.Keys);
        Assert.Equal(before, (await admin.GetTablesAsync()).Count);
    }

    [Fact(DisplayName = "003/CA-08: alterar mesa com nome vazio é recusado e a mesa não muda")]
    public async Task PutTable_Invalid_Returns400()
    {
        var admin = await AdminAsync();
        var table = (await admin.GetTablesAsync())[0];

        var response = await admin.Http.PutAsJsonAsync($"/api/table/{table.Id}", new UpdateTableDto(""), admin.Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(table.TableName, (await admin.GetTablesAsync()).Single(t => t.Id == table.Id).TableName);
    }

    [Fact(DisplayName = "003/CA-09: alterar mesa inexistente devolve não encontrado em português")]
    public async Task PutTable_Unknown_Returns404()
    {
        var admin = await AdminAsync();

        var response = await admin.Http.PutAsJsonAsync($"/api/table/{Guid.NewGuid()}", new UpdateTableDto("Mesa X"), admin.Json);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Mesa não encontrada.", (await admin.ReadProblemAsync(response)).Detail);
    }
}
