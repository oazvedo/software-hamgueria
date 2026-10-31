using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SoftwareHamgueria.API.Errors;

/// <summary>
/// Body that could not be read (malformed JSON, unknown enum value): the "invalid input" category in Portuguese,
/// without the serializer's own message (ADR 0022). Business validation happens later, in the services.
/// </summary>
public static class InvalidModelStateFactory
{
    public const string InvalidValue = "Valor inválido.";
    public const string MissingBody = "Envie os dados da requisição.";

    public static IActionResult Create(ActionContext context)
    {
        var errors = context.ModelState
            .Where(e => e.Value is { Errors.Count: > 0 })
            .GroupBy(e => FieldOf(e.Key))
            .ToDictionary(g => g.Key, g => new[] { g.Key == "body" ? MissingBody : InvalidValue });

        var problem = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Type = ProblemTypes.Validation,
            Title = ProblemTypes.ValidationTitle,
        };
        problem.Extensions["traceId"] = GlobalExceptionHandler.TraceId(context.HttpContext);

        return new BadRequestObjectResult(problem) { ContentTypes = { "application/problem+json" } };
    }

    // "$.items[0].quantity" → "items[0].quantity"; the action argument name ("dto") or "$" means the whole body.
    private static string FieldOf(string key)
    {
        var path = key.StartsWith("$.") ? key[2..] : key;
        return path is "" or "$" or "dto" ? "body" : ProblemTypes.ToJsonPath(path);
    }
}
