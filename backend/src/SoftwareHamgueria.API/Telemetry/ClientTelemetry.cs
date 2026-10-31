using System.Diagnostics.Metrics;
using SoftwareHamgueria.Application.Telemetry;

namespace SoftwareHamgueria.API.Telemetry;

/// <summary>A JavaScript error caught in the browser (spec 023).</summary>
public record ClientErrorDto(string Message, string? Stack, string? Page);

/// <summary>One Web Vitals measurement from the browser (spec 023).</summary>
public record WebVitalDto(string Name, double Value, string? Rating, string? Page);

/// <summary>Browser telemetry: errors become structured warnings in the logs, vitals a histogram (ADR 0028).</summary>
public static class ClientTelemetry
{
    public static readonly string[] VitalNames = ["LCP", "CLS", "INP", "FCP", "TTFB"];
    public static readonly string[] Ratings = ["good", "needs-improvement", "poor"];

    public static readonly Histogram<double> WebVitals = HamgueriaTelemetry.Meter.CreateHistogram<double>(
        "hamgueria.web_vitals", description: "Web Vitals medidos no navegador (ms; CLS sem unidade)");

    /// <summary>
    /// The page as a low-cardinality route: only the first path segment is kept and every other one becomes
    /// <c>:param</c>, so metrics do not split into one series per order or comanda (/order/9f3c... is /order/:param).
    /// </summary>
    public static string Route(string? page)
    {
        if (string.IsNullOrWhiteSpace(page)) return "desconhecida";
        var path = Uri.TryCreate(page, UriKind.Absolute, out var uri) ? uri.AbsolutePath : page.Split('?', '#')[0];
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0) return "/";
        return "/" + string.Join('/', segments.Take(4).Select((s, i) => i == 0 ? s[..Math.Min(s.Length, 32)] : ":param"));
    }
}
