using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SoftwareHamgueria.Application.DTOs.Auth;
using SoftwareHamgueria.Application.DTOs.Dish;
using SoftwareHamgueria.Application.DTOs.Order;
using SoftwareHamgueria.Application.DTOs.Table;
using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.FunctionalTests.Infrastructure;

/// <summary>
/// Typed wrapper over the HTTP API. Every route the tests call lives here, and the JSON options are the API's own
/// (from <see cref="HamgueriaApiFactory"/>), so the tests speak exactly what the frontend receives.
/// </summary>
public sealed class ApiClient(HttpClient http, JsonSerializerOptions json)
{
    // Development credentials from appsettings.json (Auth:AdminUsername / Auth:AdminPassword).
    public const string AdminUsername = "admin";
    public const string AdminPassword = "admin123";

    public HttpClient Http => http;

    public JsonSerializerOptions Json => json;

    /// <summary>
    /// A comanda name no other test uses, so tests never see each other's orders.
    /// Keep the prefix short: names are limited to 60 characters and the suffix takes 33.
    /// </summary>
    public static string UniqueCustomer(string prefix = "Cliente") => $"{prefix}-{Guid.NewGuid():N}";

    public Task<T?> ReadAsync<T>(HttpResponseMessage response) => response.Content.ReadFromJsonAsync<T>(json);

    /// <summary>Reads an error body, asserting it is <c>application/problem+json</c> (ADR 0022).</summary>
    public async Task<ProblemBody> ReadProblemAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = (await ReadAsync<ProblemBody>(response))!;
        Assert.Equal((int)response.StatusCode, problem.Status);
        Assert.False(string.IsNullOrEmpty(problem.TraceId));
        return problem;
    }

    // ---- auth

    public Task<HttpResponseMessage> LoginAsync(string username, string password) =>
        http.PostAsJsonAsync("/api/auth/login", new LoginDto(username, password), json);

    public async Task LoginAsAdminAsync()
    {
        var response = await LoginAsync(AdminUsername, AdminPassword);
        response.EnsureSuccessStatusCode();
        var token = await ReadAsync<TokenResponseDto>(response);
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.Token);
    }

    // ---- menu and tables

    public async Task<List<DishResponseDto>> GetDishesAsync() =>
        (await http.GetFromJsonAsync<List<DishResponseDto>>("/api/dish", json))!;

    public async Task<List<TableResponseDto>> GetTablesAsync() =>
        (await http.GetFromJsonAsync<List<TableResponseDto>>("/api/table", json))!;

    public async Task<Guid> FirstTableIdAsync() => (await GetTablesAsync())[0].Id;

    // ---- orders

    /// <summary>A valid one-item order on the first seeded table with the first active dish.</summary>
    public async Task<CreateOrderDto> BuildOrderAsync(string customer, Guid? tableId = null, int quantity = 1, string? notes = null)
    {
        var dish = (await GetDishesAsync()).First(d => d.IsActive);
        return new CreateOrderDto(tableId ?? await FirstTableIdAsync(), customer, [new OrderItemDto(dish.Id, quantity, notes)]);
    }

    public Task<HttpResponseMessage> PostOrderAsync(CreateOrderDto order) =>
        http.PostAsJsonAsync("/api/order", order, json);

    public async Task<OrderResponseDto> CreateOrderAsync(string customer, Guid? tableId = null)
    {
        var response = await PostOrderAsync(await BuildOrderAsync(customer, tableId));
        response.EnsureSuccessStatusCode();
        return (await ReadAsync<OrderResponseDto>(response))!;
    }

    public Task<HttpResponseMessage> GetOrderResponseAsync(Guid id) => http.GetAsync($"/api/order/{id}");

    public async Task<OrderResponseDto> GetOrderAsync(Guid id) =>
        (await http.GetFromJsonAsync<OrderResponseDto>($"/api/order/{id}", json))!;

    public Task<HttpResponseMessage> SetStatusAsync(Guid orderId, OrderStatus status) =>
        http.PatchAsJsonAsync($"/api/order/{orderId}/status", status, json);

    public Task<HttpResponseMessage> UpdateOrderAsync(Guid orderId, UpdateOrderDto order) =>
        http.PutAsJsonAsync($"/api/order/{orderId}", order, json);

    public Task<HttpResponseMessage> CancelAsync(Guid orderId) => http.PatchAsync($"/api/order/{orderId}/cancel", null);

    // ---- comandas

    public async Task<List<OrderResponseDto>> GetComandaAsync(Guid tableId, string customer) =>
        (await http.GetFromJsonAsync<List<OrderResponseDto>>(
            $"/api/order/comanda?tableId={tableId}&customer={Uri.EscapeDataString(customer)}", json))!;

    public async Task<List<OrderResponseDto>> GetTableOrdersAsync(Guid tableId) =>
        (await http.GetFromJsonAsync<List<OrderResponseDto>>($"/api/order/table/{tableId}", json))!;

    public Task<HttpResponseMessage> CloseComandaAsync(Guid tableId, string customer) =>
        http.PostAsJsonAsync("/api/order/comanda/close", new CloseComandaDto(tableId, customer), json);
}
