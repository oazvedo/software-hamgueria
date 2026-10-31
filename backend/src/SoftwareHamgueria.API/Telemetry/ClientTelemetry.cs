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

    /// <summary>The app's own pages (frontend routes): any other first segment is reported as <c>/outra</c>.</summary>
    public static readonly string[] KnownPages = ["menu", "order", "comanda", "comandas", "mesa", "producao"];

    /// <summary>
    /// The page as a bounded route for the metric tags (review of PR #26): the first segment only if it is one of the
    /// app's pages, every other segment as <c>:param</c>. A client cannot create new series by inventing paths.
    /// </summary>
    public static string Route(string? page)
    {
        if (string.IsNullOrWhiteSpace(page)) return "desconhecida";
        var path = Uri.TryCreate(page, UriKind.Absolute, out var uri) ? uri.AbsolutePath : page.Split('?', '#')[0];
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0) return "/";
        if (!KnownPages.Contains(segments[0])) return "/outra";
        return "/" + string.Join('/', segments.Take(4).Select((s, i) => i == 0 ? s : ":param"));
    }
}
