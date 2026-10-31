namespace SoftwareHamgueria.Application.DTOs.Audit;

public record AuditQuery(string? Table = null, string? RowId = null, int Page = 1, int PageSize = 50);

public record AuditEntryDto(
    long Id,
    string Table,
    string Operation,
    string? RowId,
    IReadOnlyList<string> ChangedFields,
    string? OldValues,
    string? NewValues,
    string Author,
    string? FrontendUrl,
    string? RequestId,
    DateTime CreatedAt);

public record AuditPageDto(IReadOnlyList<AuditEntryDto> Items, int Page, int PageSize, bool HasMore);
