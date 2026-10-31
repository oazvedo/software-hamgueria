using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SoftwareHamgueria.Application.Audit;

namespace SoftwareHamgueria.Infrastructure.Audit;

/// <summary>
/// Puts the request's author, screen and trace id into the Postgres session each time EF opens a connection, where the
/// audit trigger reads them with <c>current_setting('app.*', true)</c> (ADR 0027). Session-level (<c>is_local =
/// false</c>) so it covers EF's own transactions; Npgsql runs <c>DISCARD ALL</c> when the connection goes back to the
/// pool, so a value never leaks into the next request.
/// </summary>
public sealed class AuditContextInterceptor(IAuditContext audit) : DbConnectionInterceptor
{
    private const string Sql =
        "SELECT set_config('app.author_name', @author, false), set_config('app.frontend_url', @url, false), " +
        "set_config('app.request_id', @request, false)";

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        using var command = Build(connection);
        command.ExecuteNonQuery();
    }

    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        await using var command = Build(connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private DbCommand Build(DbConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText = Sql;
        // Empty means "no author" (an anonymous request); only a context without a request says "sistema".
        Add(command, "author", audit.AuthorName ?? string.Empty);
        Add(command, "url", audit.FrontendUrl ?? string.Empty);
        Add(command, "request", audit.RequestId ?? string.Empty);
        return command;
    }

    private static void Add(DbCommand command, string name, string value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}

/// <summary>Outside a request (migrations, seed, background jobs): the author is "sistema".</summary>
public sealed class SystemAuditContext : IAuditContext
{
    public string? AuthorName => IAuditContext.SystemAuthor;
    public string? FrontendUrl => null;
    public string? RequestId => null;
}
