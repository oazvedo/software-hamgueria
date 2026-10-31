using System.Net;
using SoftwareHamgueria.Application.DTOs.Order;
using SoftwareHamgueria.FunctionalTests.Infrastructure;

namespace SoftwareHamgueria.FunctionalTests;

// Each scenario owns its broker (BrokerApiFactory), so they live in separate classes and run in parallel.

public class QueueDownAtStartupTests
{
    [Fact(DisplayName = "005/CA-02, CA-03, CA-04, CA-07: com a fila fora desde o início, tudo funciona e os eventos saem em ordem quando ela volta")]
    public async Task BrokerDownAtStartup_EverythingWorks_EventsFlushInOrder()
    {
        await using var api = await BrokerApiFactory.StartAsync(brokerUp: false);
        var client = api.CreateApiClient();
        var customer = ApiClient.UniqueCustomer("Fila");

        // CA-02: reading and the kitchen login do not depend on the queue.
        Assert.NotEmpty(await client.GetDishesAsync());
        var tableId = await client.FirstTableIdAsync();
        Assert.Empty(await client.GetComandaAsync(tableId, customer));
        await client.LoginAsAdminAsync();
        Assert.Equal(HttpStatusCode.OK, (await client.Http.GetAsync("/api/order")).StatusCode);

        // CA-03: create, update and cancel succeed with the queue down; the events wait.
        var order = await client.CreateOrderAsync(customer, tableId);
        Assert.Equal(HttpStatusCode.OK, (await client.GetOrderResponseAsync(order.Id)).StatusCode);
        var update = new UpdateOrderDto([new OrderItemDto(order.Items[0].DishId, 2)]);
        var updated = await client.UpdateOrderAsync(order.Id, update);
        Assert.True(updated.IsSuccessStatusCode, $"{updated.StatusCode}: {await updated.Content.ReadAsStringAsync()}");
        Assert.True((await client.CancelAsync(order.Id)).IsSuccessStatusCode);

        // CA-07: main health stays healthy; details show the queue down and 3 events waiting.
        Assert.Equal("Healthy", await client.Http.GetStringAsync("/api/health"));
        var details = await api.GetHealthDetailsAsync();
        Assert.Equal("Degraded", details.GetProperty("status").GetString());
        Assert.Equal("Healthy", details.GetProperty("database").GetString());
        Assert.Equal("Unhealthy", details.GetProperty("queue").GetString());
        Assert.Equal(3, details.GetProperty("pendingEvents").GetInt32());

        // CA-04: the queue comes back, the events go out by themselves, in order.
        await api.StartBrokerAsync();
        var events = await api.ReceiveEventsAsync(3);
        var id = order.Id.ToString();
        Assert.Equal([("order.created", id), ("order.updated", id), ("order.cancelled", id)], events);
        await BrokerApiFactory.Eventually(async () =>
        {
            var after = await api.GetHealthDetailsAsync();
            return after.GetProperty("pendingEvents").GetInt32() == 0
                   && after.GetProperty("queue").GetString() == "Healthy"
                   && after.GetProperty("status").GetString() == "Healthy";
        }, TimeSpan.FromSeconds(30), "a fila de espera devia esvaziar e a saúde voltar a Healthy");
    }
}

public class QueueOutageWhileRunningTests
{
    [Fact(DisplayName = "005/CA-01, CA-05: com a fila no ar o evento sai na hora, e um pedido feito durante uma queda chega depois que ela volta")]
    public async Task BrokerUp_ThenOutage_EventsArriveAfterRecovery()
    {
        await using var api = await BrokerApiFactory.StartAsync(brokerUp: true);
        var client = api.CreateApiClient();
        var tableId = await client.FirstTableIdAsync();

        // CA-01: broker up, the event is sent right away.
        await BrokerApiFactory.Eventually(async () =>
            (await api.GetHealthDetailsAsync()).GetProperty("queue").GetString() == "Healthy",
            TimeSpan.FromSeconds(30), "o envio devia conectar na fila");
        var first = await client.CreateOrderAsync(ApiClient.UniqueCustomer("Fila"), tableId);
        Assert.Equal([("order.created", first.Id.ToString())], await api.ReceiveEventsAsync(1));

        // CA-05: the queue drops with the API running; the order still succeeds.
        await api.StopBrokerAsync();
        var during = await client.PostOrderAsync(await client.BuildOrderAsync(ApiClient.UniqueCustomer("Fila"), tableId));
        Assert.Equal(HttpStatusCode.Created, during.StatusCode);
        var second = (await client.ReadAsync<OrderResponseDto>(during))!;

        await api.StartBrokerAsync();
        Assert.Equal([("order.created", second.Id.ToString())], await api.ReceiveEventsAsync(1));
    }
}

public class QueueTelemetryTests
{
    [Fact(DisplayName = "010/CA-03: evento enviado conta em published e gera o span de envio, ligado ao broker")]
    public async Task PublishedEvent_IsCounted_AndTraced()
    {
        long published = 0;
        using var meters = new System.Diagnostics.Metrics.MeterListener
        {
            InstrumentPublished = (i, l) =>
            {
                if (i.Meter.Name == SoftwareHamgueria.Application.Telemetry.HamgueriaTelemetry.Name && i.Name == "hamgueria.order_events.published")
                    l.EnableMeasurementEvents(i);
            },
        };
        meters.SetMeasurementEventCallback<long>((_, v, _, _) => Interlocked.Add(ref published, v));
        meters.Start();
        var spans = new System.Collections.Concurrent.ConcurrentBag<System.Diagnostics.Activity>();
        using var tracer = new System.Diagnostics.ActivityListener
        {
            ShouldListenTo = s => s.Name == SoftwareHamgueria.Application.Telemetry.HamgueriaTelemetry.Name,
            Sample = (ref System.Diagnostics.ActivityCreationOptions<System.Diagnostics.ActivityContext> _) =>
                System.Diagnostics.ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = spans.Add,
        };
        System.Diagnostics.ActivitySource.AddActivityListener(tracer);

        await using var api = await BrokerApiFactory.StartAsync(brokerUp: true);
        var client = api.CreateApiClient();
        var order = await client.CreateOrderAsync(ApiClient.UniqueCustomer("Otel"));
        await api.ReceiveEventsAsync(1);

        Assert.True(Interlocked.Read(ref published) >= 1);
        var span = spans.First(a => a.DisplayName == "publish order event" && a.Status != System.Diagnostics.ActivityStatusCode.Error);
        Assert.Equal("order.created", span.GetTagItem("messaging.rabbitmq.destination.routing_key"));
        Assert.Equal(System.Diagnostics.ActivityKind.Producer, span.Kind);
    }
}
