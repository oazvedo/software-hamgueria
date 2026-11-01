using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SoftwareHamgueria.Application.Jobs;
using SoftwareHamgueria.FunctionalTests.Infrastructure;

namespace SoftwareHamgueria.FunctionalTests;

public class JobsTests(HamgueriaApiFactory factory)
{
    [Fact(DisplayName = "011/CA-01: a retenção da auditoria fica agendada como job recorrente diário")]
    public void AuditRetention_IsRecurring()
    {
        using var connection = factory.Services.GetRequiredService<JobStorage>().GetConnection();

        var job = Assert.Single(connection.GetRecurringJobs(), j => j.Id == AuditRetentionJob.Id);
        Assert.Equal("0 3 * * *", job.Cron);
    }

    [Fact(DisplayName = "011/CA-02: rodar a retenção apaga a auditoria antiga e mantém a recente")]
    public async Task AuditRetention_DeletesOldRowsOnly()
    {
        var marker = Guid.NewGuid().ToString();
        await using (var db = new NpgsqlConnection(factory.ConnectionString))
        {
            await db.OpenAsync();
            await using var insert = new NpgsqlCommand("""
                INSERT INTO audit.change_log (table_name, operation, row_id, created_at) VALUES
                    ('Orders', 'INSERT', @old, now() - interval '400 days'),
                    ('Orders', 'INSERT', @recent, now() - interval '1 day')
                """, db);
            insert.Parameters.AddWithValue("old", $"old-{marker}");
            insert.Parameters.AddWithValue("recent", $"recent-{marker}");
            await insert.ExecuteNonQueryAsync();
        }

        await using (var scope = factory.Services.CreateAsyncScope())
            Assert.True(await scope.ServiceProvider.GetRequiredService<AuditRetentionJob>().RunAsync() >= 1);

        await using var check = new NpgsqlConnection(factory.ConnectionString);
        await check.OpenAsync();
        await using var query = new NpgsqlCommand("SELECT row_id FROM audit.change_log WHERE row_id LIKE @m", check);
        query.Parameters.AddWithValue("m", $"%{marker}");
        var left = new List<string>();
        await using var reader = await query.ExecuteReaderAsync();
        while (await reader.ReadAsync()) left.Add(reader.GetString(0));
        Assert.Equal([$"recent-{marker}"], left);
    }

    [Theory(DisplayName = "011/CA-04 (falha): o painel de jobs pede as credenciais da cozinha")]
    [InlineData(null)]
    [InlineData("admin:senha-errada")]
    [InlineData("intruso:admin123")]
    public async Task Dashboard_WithoutValidCredentials_Returns401(string? credentials)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/jobs");
        if (credentials is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials)));

        var response = await factory.CreateClient().SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Contains("Basic", response.Headers.WwwAuthenticate.ToString());
    }

    [Fact(DisplayName = "011/CA-04: com as credenciais da cozinha, o painel de jobs abre")]
    public async Task Dashboard_WithAdminCredentials_Opens()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/jobs/recurring");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ApiClient.AdminUsername}:{ApiClient.AdminPassword}")));

        var response = await factory.CreateClient().SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(AuditRetentionJob.Id, await response.Content.ReadAsStringAsync());
    }
}
