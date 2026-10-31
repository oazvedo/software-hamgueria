using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using SoftwareHamgueria.Application.Telemetry;
using SoftwareHamgueria.FunctionalTests.Infrastructure;

namespace SoftwareHamgueria.FunctionalTests;

public class ObservabilityTests(HamgueriaApiFactory factory)
{
    [Fact(DisplayName = "010/CA-02: criar um pedido soma na métrica de pedidos criados")]
    public async Task CreatingAnOrder_IncrementsTheCounter()
    {
        long created = 0;
        using var listener = new MeterListener
        {
            InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter.Name == HamgueriaTelemetry.Name && instrument.Name == "hamgueria.orders.created")
                    l.EnableMeasurementEvents(instrument);
            },
        };
        listener.SetMeasurementEventCallback<long>((_, value, _, _) => Interlocked.Add(ref created, value));
        listener.Start();

        await factory.CreateApiClient().CreateOrderAsync(ApiClient.UniqueCustomer("Otel"));

        Assert.True(Interlocked.Read(ref created) >= 1);
    }

    [Fact(DisplayName = "010/CA-01: cada requisição vira um trace, com a rota e o banco como filhos")]
    public async Task Request_ProducesServerSpan_WithDatabaseChild()
    {
        var spans = new System.Collections.Concurrent.ConcurrentBag<Activity>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = s => s.Name is "Microsoft.AspNetCore" or "Npgsql",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = spans.Add,
        };
        ActivitySource.AddActivityListener(listener);

        var response = await factory.CreateApiClient().Http.GetAsync("/api/dish");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // The server span stops after the response reaches the client: wait for it instead of racing.
        await BrokerApiFactory.Eventually(() => Task.FromResult(spans.Any(a => a.Kind == ActivityKind.Server)),
            TimeSpan.FromSeconds(5), "o span do servidor devia terminar");
        var seen = string.Join(" | ", spans.Select(a => $"{a.Source.Name}/{a.Kind}/{a.DisplayName}/{a.GetTagItem("http.route")}"));
        var server = spans.LastOrDefault(a => a.Kind == ActivityKind.Server && a.GetTagItem("http.route") is "api/Dish");
        Assert.True(server is not null, seen);
        Assert.Contains(spans, a => a.Source.Name == "Npgsql" && a.TraceId == server.TraceId);
    }
}
