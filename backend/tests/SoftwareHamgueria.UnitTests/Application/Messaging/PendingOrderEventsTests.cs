using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using SoftwareHamgueria.Application.Messaging;

namespace SoftwareHamgueria.UnitTests.Application.Messaging;

public class PendingOrderEventsTests
{
    private readonly FakeLogger<PendingOrderEvents> _logger = new();

    private static PendingOrderEvent Event(int n) => new("order.created", $"pedido-{n}");

    [Fact(DisplayName = "005/CA-04: os eventos saem na ordem em que entraram")]
    public void Events_LeaveInArrivalOrder()
    {
        var sut = new PendingOrderEvents(_logger);
        for (var i = 1; i <= 3; i++) sut.Enqueue(Event(i));

        var sent = new List<string>();
        while (sut.TryPeek(out var head))
        {
            sent.Add(head!.Message);
            Assert.True(sut.RemoveHead(head));
        }

        Assert.Equal(["pedido-1", "pedido-2", "pedido-3"], sent);
        Assert.Equal(0, sut.Count);
    }

    [Fact(DisplayName = "005/CA-06: com 1000 esperando, o próximo entra e o mais antigo é descartado com log")]
    public void Full_DropsOldestAndLogsIt()
    {
        var sut = new PendingOrderEvents(_logger);
        for (var i = 1; i <= PendingOrderEvents.DefaultCapacity; i++) sut.Enqueue(Event(i));

        sut.Enqueue(Event(1001));

        Assert.Equal(PendingOrderEvents.DefaultCapacity, sut.Count);
        Assert.True(sut.TryPeek(out var head));
        Assert.Equal("pedido-2", head!.Message);
        var log = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Warning, log.Level);
        Assert.Contains("pedido-1", log.Message);
    }

    [Fact(DisplayName = "005/CA-06: abaixo do limite, nada é descartado")]
    public void BelowCapacity_DropsNothing()
    {
        var sut = new PendingOrderEvents(_logger);
        for (var i = 1; i <= PendingOrderEvents.DefaultCapacity; i++) sut.Enqueue(Event(i));

        Assert.Equal(PendingOrderEvents.DefaultCapacity, sut.Count);
        Assert.Empty(_logger.Collector.GetSnapshot());
    }

    [Fact(DisplayName = "005/RN-03: um evento descartado durante o envio não tira o seguinte da fila")]
    public void RemoveHead_OfDroppedEvent_RemovesNothing()
    {
        var sut = new PendingOrderEvents(_logger, capacity: 2);
        sut.Enqueue(Event(1));
        sut.Enqueue(Event(2));
        Assert.True(sut.TryPeek(out var sending));

        sut.Enqueue(Event(3)); // drops pedido-1 while it is being sent

        Assert.False(sut.RemoveHead(sending!));
        Assert.Equal(2, sut.Count);
        Assert.True(sut.TryPeek(out var head));
        Assert.Equal("pedido-2", head!.Message);
    }

    [Fact(DisplayName = "005/RN-03: o envio acorda quando chega um evento")]
    public async Task WaitForEvents_CompletesOnEnqueue()
    {
        var sut = new PendingOrderEvents(_logger);
        var waiting = sut.WaitForEventsAsync(TestContext.Current.CancellationToken);
        Assert.False(waiting.IsCompleted);

        sut.Enqueue(Event(1));

        await waiting.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }
}
