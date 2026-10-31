using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging.Abstractions;
using SoftwareHamgueria.Application.Messaging;
using SoftwareHamgueria.Application.Telemetry;

namespace SoftwareHamgueria.UnitTests.Application.Messaging;

/// <summary>
/// The pending-events gauge is one per process and follows the latest queue (review of PR #11), so these tests must
/// not run while other tests create queues in parallel.
/// </summary>
[CollectionDefinition(nameof(TelemetryTests), DisableParallelization = true)]
public class TelemetryCollection;

[Collection(nameof(TelemetryTests))]
public class TelemetryTests
{
    private static MeterListener Listen(Action<string, long, object?> onMeasurement)
    {
        var listener = new MeterListener
        {
            InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter.Name == HamgueriaTelemetry.Name) l.EnableMeasurementEvents(instrument);
            },
        };
        listener.SetMeasurementEventCallback<long>((i, v, tags, _) => onMeasurement(i.Name, v, tags.ToArray().FirstOrDefault(t => t.Key == "reason").Value));
        listener.SetMeasurementEventCallback<int>((i, v, _, _) => onMeasurement(i.Name, v, null));
        listener.Start();
        return listener;
    }

    [Fact(DisplayName = "010/CA-03: evento descartado por fila cheia vira métrica com o motivo")]
    public void Drop_IsCounted_WithReason()
    {
        var dropped = new List<object?>();
        using var listener = Listen((name, _, reason) => { if (name == "hamgueria.order_events.dropped") dropped.Add(reason); });
        var sut = new PendingOrderEvents(NullLogger<PendingOrderEvents>.Instance, capacity: 1);

        sut.Enqueue(new("order.created", "a"));
        sut.Enqueue(new("order.created", "b"));

        Assert.Contains("queue_full", dropped);
    }

    [Fact(DisplayName = "010/CA-03: o tamanho da fila de espera é uma métrica observável")]
    public void Pending_IsObservable()
    {
        var seen = new List<long>();
        using var listener = Listen((name, value, _) => { if (name == "hamgueria.order_events.pending") seen.Add(value); });
        var sut = new PendingOrderEvents(NullLogger<PendingOrderEvents>.Instance);
        sut.Enqueue(new("order.created", "a"));
        sut.Enqueue(new("order.created", "b"));

        listener.RecordObservableInstruments();

        Assert.Contains(2, seen);
    }
}
