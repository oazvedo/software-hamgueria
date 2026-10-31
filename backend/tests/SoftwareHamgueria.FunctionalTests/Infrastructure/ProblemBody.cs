namespace SoftwareHamgueria.FunctionalTests.Infrastructure;

/// <summary>The ProblemDetails body every API error returns (ADR 0022).</summary>
public sealed record ProblemBody(
    string Type,
    string Title,
    int Status,
    string? Detail,
    string? TraceId,
    Dictionary<string, string[]>? Errors)
{
    public const string Validation = "https://hamgueria.dev/erros/validacao";
    public const string BusinessRule = "https://hamgueria.dev/erros/regra";
    public const string NotFound = "https://hamgueria.dev/erros/nao-encontrado";
    public const string Unauthorized = "https://hamgueria.dev/erros/sem-login";
    public const string Failure = "https://hamgueria.dev/erros/falha";
}
