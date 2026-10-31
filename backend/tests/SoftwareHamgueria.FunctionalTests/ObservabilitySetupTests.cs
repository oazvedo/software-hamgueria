using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using SoftwareHamgueria.API.Observability;

namespace SoftwareHamgueria.FunctionalTests;

/// <summary>The wiring of <see cref="ObservabilityExtensions"/> per environment (spec 010, CA-05 and CA-06).</summary>
public class ObservabilitySetupTests
{
    private static IServiceCollection Services(string environment, string? endpoint)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = environment });
        builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] = endpoint;
        builder.AddObservability();
        return builder.Services;
    }

    private static IServiceProvider Build(string environment, string? endpoint) =>
        Services(environment, endpoint).BuildServiceProvider();

    // AddOtlpExporter registers its options types (OtlpExporterOptions and friends); without it, none exist.
    private static bool RegistersOtlpExporter(string? endpoint) =>
        Services("Production", endpoint).Any(d =>
            (d.ServiceType.GenericTypeArguments.FirstOrDefault()?.FullName ?? d.ServiceType.FullName ?? "").Contains("Otlp"));

    [Fact(DisplayName = "010/CA-05 (falha): sem coletor configurado, nenhum exportador OTLP é registrado")]
    public void WithoutEndpoint_NoExporter()
    {
        Assert.False(RegistersOtlpExporter(null));
        Assert.False(RegistersOtlpExporter(""));
    }

    [Fact(DisplayName = "010/CA-05: com o coletor configurado, o exportador OTLP é registrado")]
    public void WithEndpoint_RegistersExporter()
    {
        Assert.True(RegistersOtlpExporter("http://collector:4317"));
    }

    [Fact(DisplayName = "010/CA-06: fora de Development o log do console sai em JSON; em Development, não")]
    public void ProductionConsole_IsJson()
    {
        string Formatter(string env) =>
            Build(env, null).GetRequiredService<IOptionsMonitor<ConsoleLoggerOptions>>().CurrentValue.FormatterName ?? "";

        Assert.Equal(ConsoleFormatterNames.Json, Formatter("Production"));
        Assert.NotEqual(ConsoleFormatterNames.Json, Formatter("Development"));
    }
}
