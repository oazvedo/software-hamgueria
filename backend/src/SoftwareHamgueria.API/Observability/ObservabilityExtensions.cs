using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SoftwareHamgueria.Application.Telemetry;

namespace SoftwareHamgueria.API.Observability;

/// <summary>
/// OpenTelemetry for traces, metrics and logs (ADR 0028). Instrumentation always runs (so tests and local runs see the
/// same activities and meters); the OTLP exporter is added only when <c>OTEL_EXPORTER_OTLP_ENDPOINT</c> is set, which
/// the dev appsettings and the compose files do. Outside Development, console logs are JSON.
/// </summary>
public static class ObservabilityExtensions
{
    public const string ServiceName = "hamgueria-api";

    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        var exportToOtlp = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
        var version = typeof(ObservabilityExtensions).Assembly.GetName().Version?.ToString() ?? "0.0.0";

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r
                .AddService(ServiceName, serviceVersion: version)
                .AddAttributes([new("deployment.environment.name", builder.Environment.EnvironmentName)]))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(HamgueriaTelemetry.Name)
                    .AddAspNetCoreInstrumentation(o =>
                        // Compose probes /api/health every few seconds: noise, not traffic.
                        o.Filter = ctx => !ctx.Request.Path.StartsWithSegments("/api/health"))
                    .AddHttpClientInstrumentation()
                    .AddNpgsql();
                if (exportToOtlp) tracing.AddOtlpExporter();
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter(HamgueriaTelemetry.Name)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddNpgsqlInstrumentation();
                if (exportToOtlp) metrics.AddOtlpExporter();
            });

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
            if (exportToOtlp) logging.AddOtlpExporter();
        });

        if (!builder.Environment.IsDevelopment())
            builder.Logging.AddJsonConsole(o => o.IncludeScopes = true);

        return builder;
    }
}
