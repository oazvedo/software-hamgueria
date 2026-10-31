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

    // One gauge for the whole process, created once. It reads the live queue through a weak reference, so a new
    // PendingOrderEvents (each test host) replaces the source instead of adding instruments (review of PR #11).
    private static WeakReference<Func<int>>? _pendingSource;

    public static readonly ObservableGauge<int> OrderEventsPending = Meter.CreateObservableGauge(
        "hamgueria.order_events.pending",
        () => _pendingSource is not null && _pendingSource.TryGetTarget(out var read) ? read() : 0,
        unit: "{event}",
        description: "Eventos esperando envio ao RabbitMQ");

    public static void TrackPendingEvents(Func<int> read) => _pendingSource = new WeakReference<Func<int>>(read);

    public static readonly Counter<long> OrderEventsDropped =
        Meter.CreateCounter<long>("hamgueria.order_events.dropped", unit: "{event}", description: "Eventos descartados (fila de espera cheia ou recusados)");
}
