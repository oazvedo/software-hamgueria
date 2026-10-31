using System.Collections.Concurrent;
using SoftwareHamgueria.Application.Messaging;

namespace SoftwareHamgueria.FunctionalTests.Infrastructure;

/// <summary>Records published events instead of talking to RabbitMQ.</summary>
public sealed class FakeOrderEventPublisher : IOrderEventPublisher
{
    private readonly ConcurrentQueue<(string RoutingKey, string Message)> _events = new();

    public void Publish(string routingKey, string message) => _events.Enqueue((routingKey, message));

    public bool WasPublished(string routingKey, string message) => _events.Contains((routingKey, message));
}
