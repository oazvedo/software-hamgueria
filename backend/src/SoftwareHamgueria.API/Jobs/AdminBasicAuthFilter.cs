using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Hangfire.Dashboard;

namespace SoftwareHamgueria.API.Jobs;

/// <summary>
/// The job dashboard is a server-rendered page, so the kitchen's JWT (sent by the SPA in a header) never reaches it.
/// It asks for the same admin credentials by HTTP Basic instead (ADR 0029), compared in constant time. Only over HTTPS
/// in production (Caddy, ADR 0023).
/// </summary>
public sealed class AdminBasicAuthFilter(IConfiguration configuration) : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var http = context.GetHttpContext();
        if (IsAdmin(http.Request.Headers.Authorization.ToString())) return true;
        http.Response.Headers.WWWAuthenticate = "Basic realm=\"Hamgueria jobs\", charset=\"UTF-8\"";
        return false;
    }

    private bool IsAdmin(string header)
    {
        var user = configuration["Auth:AdminUsername"];
        var password = configuration["Auth:AdminPassword"];
        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password)) return false;
        if (!AuthenticationHeaderValue.TryParse(header, out var value) || value.Scheme != "Basic" || value.Parameter is null) return false;

        string decoded;
        try { decoded = Encoding.UTF8.GetString(Convert.FromBase64String(value.Parameter)); }
        catch (FormatException) { return false; }

        var expected = Encoding.UTF8.GetBytes($"{user}:{password}");
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(decoded), expected);
    }
}
