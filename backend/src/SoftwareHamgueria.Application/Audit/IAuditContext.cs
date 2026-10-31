namespace SoftwareHamgueria.Application.Audit;

/// <summary>
/// Who is changing data and from which screen, for the database audit trail (ADR 0027). The API fills it from the
/// request; outside a request (migrations, seed, jobs) the author is <see cref="SystemAuthor"/>.
/// </summary>
public interface IAuditContext
{
    public const string SystemAuthor = "sistema";

    string? AuthorName { get; }
    string? FrontendUrl { get; }
    string? RequestId { get; }
}
