using System.Net.Http.Json;
using SoftwareHamgueria.Application.DTOs.Dish;
using SoftwareHamgueria.Domain.Entities;
using SoftwareHamgueria.FunctionalTests.Infrastructure;

namespace SoftwareHamgueria.FunctionalTests;

public class ActiveDishesTests(HamgueriaApiFactory factory)
{
    [Fact(DisplayName = "024/CA-01: o cardápio público pede só os pratos ativos; a cozinha continua vendo todos")]
    public async Task ActiveFilter_HidesDisabledDishes()
    {
        var admin = factory.CreateApiClient();
        await admin.LoginAsAdminAsync();
        var created = await admin.Http.PostAsJsonAsync("/api/dish",
            new CreateDishDto($"Desativado {Guid.NewGuid():N}"[..30], "teste", 9.90m, Category.Drinks), admin.Json);
        var dish = (await admin.ReadAsync<DishResponseDto>(created))!;
        (await admin.Http.PatchAsync($"/api/dish/{dish.Id}/disable", null)).EnsureSuccessStatusCode();
        var api = factory.CreateApiClient();

        var active = (await api.Http.GetFromJsonAsync<List<DishResponseDto>>("/api/dish?active=true", api.Json))!;
        var all = (await api.Http.GetFromJsonAsync<List<DishResponseDto>>("/api/dish", api.Json))!;

        Assert.DoesNotContain(active, d => d.Id == dish.Id);
        Assert.All(active, d => Assert.True(d.IsActive));
        Assert.Contains(all, d => d.Id == dish.Id);
    }
}
