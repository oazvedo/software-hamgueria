using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace SoftwareHamgueria.Application.Telemetry;

/// <summary>
/// The app's own traces and metrics (ADR 0028), on top of what ASP.NET Core, HttpClient and Npgsql already emit.
/// BCL types only, so the Application stays free of OpenTelemetry; the API wires the exporters.
/// </summary>
public static class HamgueriaTelemetry
{
    public const string Name = "SoftwareHamgueria";

    public static readonly ActivitySource Source = new(Name);
    public static readonly Meter Meter = new(Name);

    public static readonly Counter<long> OrdersCreated =
        Meter.CreateCounter<long>("hamgueria.orders.created", unit: "{order}", description: "Pedidos criados");

    public static readonly Counter<long> OrderEventsPublished =
        Meter.CreateCounter<long>("hamgueria.order_events.published", unit: "{event}", description: "Eventos confirmados pelo RabbitMQ");

    public static readonly Counter<long> OrderEventsDropped =
        Meter.CreateCounter<long>("hamgueria.order_events.dropped", unit: "{event}", description: "Eventos descartados (fila de espera cheia ou recusados)");
}
