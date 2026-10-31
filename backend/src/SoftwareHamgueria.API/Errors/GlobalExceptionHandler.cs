using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SoftwareHamgueria.Application.Exceptions;

namespace SoftwareHamgueria.API.Errors;

/// <summary>Turns every exception into ProblemDetails; unexpected ones never leak their message (ADR 0022).</summary>
public sealed class GlobalExceptionHandler(IProblemDetailsService problemDetails, ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception exception, CancellationToken ct)
    {
        ProblemDetails problem = exception switch
        {
            ValidationException ve => new HttpValidationProblemDetails(
                ve.Errors
                    .GroupBy(e => ProblemTypes.ToJsonPath(e.PropertyName))
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray()))
            {
                Status = StatusCodes.Status400BadRequest,
                Type = ProblemTypes.Validation,
                Title = ProblemTypes.ValidationTitle,
            },
            BusinessRuleException br => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Type = ProblemTypes.BusinessRule,
                Title = ProblemTypes.BusinessRuleTitle,
                Detail = br.Message,
            },
            NotFoundException nf => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Type = ProblemTypes.NotFound,
                Title = ProblemTypes.NotFoundTitle,
                Detail = nf.Message,
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Type = ProblemTypes.Failure,
                Title = ProblemTypes.FailureTitle,
            },
        };

        if (problem.Status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unexpected failure. traceId={TraceId}", TraceId(http));

        http.Response.StatusCode = problem.Status!.Value;
        // The exception is deliberately not passed on: no writer may copy its message or stack into the body.
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext { HttpContext = http, ProblemDetails = problem });
    }

    public static string TraceId(HttpContext http) =>
        System.Diagnostics.Activity.Current?.Id ?? http.TraceIdentifier;
}
