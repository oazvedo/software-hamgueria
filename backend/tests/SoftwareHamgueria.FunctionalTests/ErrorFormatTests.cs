using System.Net;
using System.Net.Http.Headers;
using System.Text;
using SoftwareHamgueria.FunctionalTests.Infrastructure;

namespace SoftwareHamgueria.FunctionalTests;

public class ErrorFormatTests(HamgueriaApiFactory factory)
{
    private readonly ApiClient _api = factory.CreateApiClient();

    private static StringContent Json(string body) => new(body, Encoding.UTF8, "application/json");

    [Fact(DisplayName = "003/CA-10: falha inesperada devolve mensagem neutra e código de ocorrência, sem detalhe interno")]
    public async Task UnexpectedFailure_IsNeutral()
    {
        var response = await _api.Http.GetAsync("/__test/boom");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var raw = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(BoomController.Secret, raw);
        Assert.DoesNotContain("InvalidOperationException", raw);
        Assert.DoesNotContain("BoomController", raw);
        var problem = await _api.ReadProblemAsync(response);
        Assert.Equal((ProblemBody.Failure, "Algo deu errado do nosso lado. Tente de novo."), (problem.Type, problem.Title));
        Assert.Null(problem.Detail);
    }

    [Fact(DisplayName = "003/RN-07: sem login, a recusa vem no formato padrão")]
    public async Task WithoutLogin_IsUnauthorizedProblem()
    {
        var response = await _api.Http.GetAsync("/api/order");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(ProblemBody.Unauthorized, (await _api.ReadProblemAsync(response)).Type);
    }

    [Fact(DisplayName = "001/CA-10: credenciais erradas são negadas no formato padrão, em português")]
    public async Task WrongCredentials_IsUnauthorizedProblem()
    {
        var response = await _api.LoginAsync("admin", "errada");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var problem = await _api.ReadProblemAsync(response);
        Assert.Equal((ProblemBody.Unauthorized, "Usuário ou senha inválidos."), (problem.Type, problem.Title));
    }

    [Fact(DisplayName = "003/RN-07: registro inexistente vem no formato padrão")]
    public async Task UnknownRecord_IsNotFoundProblem()
    {
        var response = await _api.GetOrderResponseAsync(Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(ProblemBody.NotFound, (await _api.ReadProblemAsync(response)).Type);
    }

    [Fact(DisplayName = "003/RN-06: JSON malformado é entrada inválida em português, sem a mensagem do leitor de JSON")]
    public async Task MalformedJson_IsValidationProblem()
    {
        var response = await _api.Http.PostAsync("/api/order", Json("""{"tableId": "x", "items": [ """));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var raw = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("JSON", raw);
        var problem = await _api.ReadProblemAsync(response);
        Assert.Equal(ProblemBody.Validation, problem.Type);
        Assert.NotEmpty(problem.Errors!);
        Assert.All(problem.Errors!.Values.SelectMany(v => v), m => Assert.Contains(m, new[] { "Valor inválido.", "Envie os dados da requisição." }));
    }

    [Fact(DisplayName = "003/RN-06: corpo vazio é entrada inválida em português")]
    public async Task EmptyBody_IsValidationProblem()
    {
        var response = await _api.Http.PostAsync("/api/order", Json(""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(ProblemBody.Validation, (await _api.ReadProblemAsync(response)).Type);
    }

    [Fact(DisplayName = "003/RN-06: status desconhecido do pedido é entrada inválida no campo")]
    public async Task UnknownEnum_IsValidationProblem()
    {
        var admin = factory.CreateApiClient();
        await admin.LoginAsAdminAsync();
        var order = await _api.CreateOrderAsync(ApiClient.UniqueCustomer());

        var response = await admin.Http.PatchAsync($"/api/order/{order.Id}/status", Json("\"Queimado\""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await admin.ReadProblemAsync(response);
        Assert.Equal(ProblemBody.Validation, problem.Type);
        Assert.DoesNotContain("Queimado", await response.Content.ReadAsStringAsync());
    }
}
