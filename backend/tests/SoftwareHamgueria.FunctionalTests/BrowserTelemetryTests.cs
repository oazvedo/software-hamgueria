using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using SoftwareHamgueria.API.Telemetry;
using SoftwareHamgueria.Application.Telemetry;
using SoftwareHamgueria.FunctionalTests.Infrastructure;

namespace SoftwareHamgueria.FunctionalTests;

public class BrowserTelemetryTests(HamgueriaApiFactory factory)
{
    [Fact(DisplayName = "023/CA-01: o trace da API continua o traceparent que o navegador mandou")]
    public async Task Traceparent_IsJoined()
    {
        var traceId = ActivityTraceId.CreateRandom().ToHexString();
        var seen = new System.Collections.Concurrent.ConcurrentBag<Activity>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = s => s.Name == "Microsoft.AspNetCore",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = seen.Add,
        };
        ActivitySource.AddActivityListener(listener);
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/dish");
        request.Headers.Add("traceparent", $"00-{traceId}-{ActivitySpanId.CreateRandom().ToHexString()}-01");

        (await factory.CreateClient().SendAsync(request)).EnsureSuccessStatusCode();

        await BrokerApiFactory.Eventually(() => Task.FromResult(seen.Any(a => a.TraceId.ToHexString() == traceId)),
            TimeSpan.FromSeconds(5), "o span do servidor devia continuar o trace do navegador");
    }

    [Fact(DisplayName = "023/CA-02: erro do navegador é aceito")]
    public async Task ClientError_IsAccepted()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/telemetry/client-error",
            new ClientErrorDto("quebrou", "at x", "http://localhost/menu"));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Theory(DisplayName = "023/CA-05 (falha): telemetria inválida é recusada sem eco")]
    [InlineData("client-error", """{"message":""}""")]
    [InlineData("vitals", """{"name":"XYZ","value":1}""")]
    [InlineData("vitals", """{"name":"LCP","value":-1}""")]
    public async Task InvalidTelemetry_Returns400(string path, string json)
    {
        var response = await factory.CreateClient().PostAsync($"/api/telemetry/{path}",
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "023/CA-04: Web Vitals vira métrica com a rota sem ids")]
    public async Task Vital_IsRecorded_WithLowCardinalityRoute()
    {
        var routes = new System.Collections.Concurrent.ConcurrentBag<string>();
        using var meters = new MeterListener
        {
            InstrumentPublished = (i, l) =>
            {
                if (i.Meter.Name == HamgueriaTelemetry.Name && i.Name == "hamgueria.web_vitals") l.EnableMeasurementEvents(i);
            },
        };
        meters.SetMeasurementEventCallback<double>((_, _, tags, _) =>
        {
            foreach (var t in tags) if (t.Key == "route") routes.Add((string)t.Value!);
        });
        meters.Start();

        var response = await factory.CreateClient().PostAsJsonAsync("/api/telemetry/vitals",
            new WebVitalDto("LCP", 1800, "good", "http://localhost:5173/order/9f3c2a1b-0000-4000-8000-000000000000?x=1"));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Contains("/order/:param", routes);
    }

    [Fact(DisplayName = "023/CA-06 (falha): passou do limite por minuto, a telemetria responde 429")]
    public async Task Telemetry_IsRateLimited()
    {
        using var limited = factory.WithWebHostBuilder(b => b.UseSetting("Telemetry:PermitPerMinute", "3"));
        var client = limited.CreateClient();
        var statuses = new List<HttpStatusCode>();
        for (var i = 0; i < 5; i++)
            statuses.Add((await client.PostAsJsonAsync("/api/telemetry/client-error", new ClientErrorDto("e", null, null))).StatusCode);

        Assert.Equal([HttpStatusCode.NoContent, HttpStatusCode.NoContent, HttpStatusCode.NoContent,
            HttpStatusCode.TooManyRequests, HttpStatusCode.TooManyRequests], statuses);
    }

    [Fact(DisplayName = "023/CA-06: atrás do proxy, cada cliente tem o próprio limite (X-Forwarded-For)")]
    public async Task RateLimit_IsPerForwardedClient()
    {
        using var limited = factory.WithWebHostBuilder(b => b.UseSetting("Telemetry:PermitPerMinute", "2"));
        var client = limited.CreateClient();
        async Task<HttpStatusCode> Post(string ip)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/telemetry/client-error")
            {
                Content = JsonContent.Create(new ClientErrorDto("e", null, null)),
            };
            request.Headers.Add("X-Forwarded-For", ip);
            return (await client.SendAsync(request)).StatusCode;
        }

        var a = new[] { await Post("203.0.113.10"), await Post("203.0.113.10"), await Post("203.0.113.10") };
        var b = await Post("203.0.113.20");

        Assert.Equal([HttpStatusCode.NoContent, HttpStatusCode.NoContent, HttpStatusCode.TooManyRequests], a);
        Assert.Equal(HttpStatusCode.NoContent, b);
    }
}
