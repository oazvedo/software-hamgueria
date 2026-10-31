using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using SoftwareHamgueria.Application.Jobs;
using SoftwareHamgueria.Application.Repositories;

namespace SoftwareHamgueria.UnitTests.Application.Jobs;

public class AuditRetentionJobTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 31, 3, 0, 0, TimeSpan.Zero);
    private readonly IAuditRepository _repository = Substitute.For<IAuditRepository>();

    private AuditRetentionJob Job(string? days) => new(
        _repository,
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Jobs:AuditRetentionDays"] = days }).Build(),
        new FakeTimeProvider(Now),
        NullLogger<AuditRetentionJob>.Instance);

    [Theory(DisplayName = "011/CA-02: a retenção apaga o que passou do prazo configurado")]
    [InlineData(null, 180)]
    [InlineData("30", 30)]
    [InlineData("7", 7)]
    public async Task Deletes_OlderThanTheConfiguredDays(string? configured, int expectedDays)
    {
        _repository.DeleteOlderThanAsync(Arg.Any<DateTime>()).Returns(4);

        var deleted = await Job(configured).RunAsync();

        Assert.Equal(4, deleted);
        await _repository.Received(1).DeleteOlderThanAsync(Now.UtcDateTime.AddDays(-expectedDays));
    }

    [Theory(DisplayName = "011/CA-03 (falha): prazo inválido ou curto demais cai no padrão de 180 dias")]
    [InlineData("6")]
    [InlineData("0")]
    [InlineData("-10")]
    [InlineData("abc")]
    public async Task InvalidDays_FallBackToDefault(string configured)
    {
        await Job(configured).RunAsync();

        await _repository.Received(1).DeleteOlderThanAsync(Now.UtcDateTime.AddDays(-AuditRetentionJob.DefaultDays));
    }
}
