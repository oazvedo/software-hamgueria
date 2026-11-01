namespace SoftwareHamgueria.Domain.Entities;

/// <summary>
/// One row of <c>audit.change_log</c>, written only by the database trigger <c>audit.log_changes()</c> (ADR 0027).
/// Read-only for the application: no constructor besides EF's, no setters.
/// </summary>
public class AuditEntry
{
    public long Id { get; private set; }
    public string TableName { get; private set; } = string.Empty;
    public string Operation { get; private set; } = string.Empty;
    public string? RowId { get; private set; }
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public string[] ChangedFields { get; private set; } = [];
    public string? AuthorName { get; private set; }
    public string? FrontendUrl { get; private set; }
    public string? RequestId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected AuditEntry() { }
}
