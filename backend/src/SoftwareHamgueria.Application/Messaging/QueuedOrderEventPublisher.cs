namespace SoftwareHamgueria.Application.Messaging;

/// <summary>Only queues the event; a background sender delivers it, so a broker outage never fails the request (ADR 0025).</summary>
public sealed class QueuedOrderEventPublisher(PendingOrderEvents pending) : IOrderEventPublisher
{
    public void Publish(string routingKey, string message) => pending.Enqueue(new PendingOrderEvent(routingKey, message));
}
