using Microsoft.Extensions.Logging.Abstractions;
using SoftwareHamgueria.Application.Messaging;

namespace SoftwareHamgueria.UnitTests.Application.Messaging;

public class QueuedOrderEventPublisherTests
{
    [Fact(DisplayName = "005/CA-03: publicar só enfileira, sem depender da fila de mensagens")]
    public void Publish_OnlyQueues()
    {
        var pending = new PendingOrderEvents(NullLogger<PendingOrderEvents>.Instance);
        var sut = new QueuedOrderEventPublisher(pending);

        sut.Publish("order.created", "pedido-1");

        Assert.True(pending.TryPeek(out var head));
        Assert.Equal(new PendingOrderEvent("order.created", "pedido-1"), head);
    }
}
