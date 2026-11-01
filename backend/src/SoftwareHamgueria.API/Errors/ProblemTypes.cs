using Microsoft.AspNetCore.Http;

namespace SoftwareHamgueria.API.Errors;

/// <summary>Problem <c>type</c> and Portuguese <c>title</c> per error category (ADR 0022).</summary>
public static class ProblemTypes
{
    private const string Base = "https://hamgueria.dev/erros/";

    public const string Validation = Base + "validacao";
    public const string BusinessRule = Base + "regra";
    public const string NotFound = Base + "nao-encontrado";
    public const string Unauthorized = Base + "sem-login";
    public const string Forbidden = Base + "sem-permissao";
    public const string Failure = Base + "falha";

    public const string ValidationTitle = "Alguns dados não passaram na checagem.";
    public const string BusinessRuleTitle = "O restaurante não aceitou essa ação.";
    public const string NotFoundTitle = "Não encontramos o que você pediu.";
    public const string UnauthorizedTitle = "Faça login na cozinha para continuar.";
    public const string ForbiddenTitle = "Seu login não tem permissão para isso.";
    public const string FailureTitle = "Algo deu errado do nosso lado. Tente de novo.";
    public const string InvalidCredentialsTitle = "Usuário ou senha inválidos.";

    /// <summary>Category for a bare status code (e.g. <c>NotFound()</c> or the JWT challenge).</summary>
    public static (string Type, string Title) ForStatus(int status) => status switch
    {
        StatusCodes.Status400BadRequest => (Validation, ValidationTitle),
        StatusCodes.Status401Unauthorized => (Unauthorized, UnauthorizedTitle),
        StatusCodes.Status403Forbidden => (Forbidden, ForbiddenTitle),
        StatusCodes.Status404NotFound => (NotFound, NotFoundTitle),
        _ => (Failure, FailureTitle),
    };

    /// <summary>"Items[1].Quantity" → "items[1].quantity", matching the JSON the frontend sends.</summary>
    public static string ToJsonPath(string propertyName) =>
        string.Join('.', propertyName.Split('.').Select(s => s.Length == 0 ? s : char.ToLowerInvariant(s[0]) + s[1..]));
}
