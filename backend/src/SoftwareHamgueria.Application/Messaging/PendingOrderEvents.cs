using Microsoft.Extensions.Logging;

namespace SoftwareHamgueria.Application.Messaging;

public sealed record PendingOrderEvent(string RoutingKey, string Message);

/// <summary>
/// Order events waiting to be sent to the broker (ADR 0025): FIFO, bounded, in memory.
/// When full, the oldest event is dropped and logged. The sender peeks the head, sends it and only then removes it.
/// </summary>
public sealed class PendingOrderEvents
{
    public const int DefaultCapacity = 1000;

    private readonly Queue<PendingOrderEvent> _events = new();
    private readonly Lock _lock = new();
    private readonly SemaphoreSlim _signal = new(0);
    private readonly ILogger<PendingOrderEvents> _logger;
    private readonly int _capacity;

    public PendingOrderEvents(ILogger<PendingOrderEvents> logger, int capacity = DefaultCapacity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 1);
        _logger = logger;
        _capacity = capacity;
    }

    public int Count
    {
        get { lock (_lock) return _events.Count; }
    }

    public void Enqueue(PendingOrderEvent orderEvent)
    {
        PendingOrderEvent? dropped = null;
        lock (_lock)
        {
            if (_events.Count >= _capacity) dropped = _events.Dequeue();
            _events.Enqueue(orderEvent);
        }

        if (dropped is not null)
            _logger.LogWarning(
                "Order event queue full ({Capacity}): dropped {RoutingKey} for order {OrderId}",
                _capacity, dropped.RoutingKey, dropped.Message);

        // Wake the sender; one pending release is enough, it drains everything it finds.
        if (_signal.CurrentCount == 0) _signal.Release();
    }

    public bool TryPeek(out PendingOrderEvent? orderEvent)
    {
        lock (_lock) return _events.TryPeek(out orderEvent);
    }

    /// <summary>Removes <paramref name="orderEvent"/> only if it is still the head (it may have been dropped meanwhile).</summary>
    public bool RemoveHead(PendingOrderEvent orderEvent)
    {
        lock (_lock)
        {
            if (!_events.TryPeek(out var head) || !ReferenceEquals(head, orderEvent)) return false;
            _events.Dequeue();
            return true;
        }
    }

    /// <summary>Completes when there is at least one event waiting.</summary>
    public async Task WaitForEventsAsync(CancellationToken cancellationToken)
    {
        while (Count == 0)
            await _signal.WaitAsync(cancellationToken);
    }
}
