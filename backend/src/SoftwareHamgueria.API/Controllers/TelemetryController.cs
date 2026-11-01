using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SoftwareHamgueria.API.Telemetry;

namespace SoftwareHamgueria.API.Controllers;

/// <summary>
/// Telemetry sent by the browser (spec 023). Public (customers are anonymous) but rate limited per IP; it only writes
/// logs and metrics, never the database. Bad input is a plain 400: no detail is echoed back.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting(Policy)]
public class TelemetryController(ILogger<TelemetryController> logger) : ControllerBase
{
    public const string Policy = "telemetry";

    [HttpPost("client-error")]
    public IActionResult ClientError([FromBody] ClientErrorDto error)
    {
        if (string.IsNullOrWhiteSpace(error.Message) || error.Message.Length > 500) return BadRequest();
        var stack = error.Stack is { Length: > 2000 } ? error.Stack[..2000] : error.Stack;
        logger.LogWarning("Browser error on {Route}: {Message} {Stack}", ClientTelemetry.Route(error.Page), error.Message, stack);
        return NoContent();
    }

    [HttpPost("vitals")]
    public IActionResult Vitals([FromBody] WebVitalDto vital)
    {
        if (!ClientTelemetry.VitalNames.Contains(vital.Name) || vital.Value < 0 || vital.Value > 600_000 || double.IsNaN(vital.Value))
            return BadRequest();
        var rating = ClientTelemetry.Ratings.Contains(vital.Rating) ? vital.Rating! : "unknown";
        ClientTelemetry.WebVitals.Record(vital.Value,
            new KeyValuePair<string, object?>("name", vital.Name),
            new KeyValuePair<string, object?>("rating", rating),
            new KeyValuePair<string, object?>("route", ClientTelemetry.Route(vital.Page)));
        return NoContent();
    }
}
