using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.Application.Repositories;

public interface IAuditRepository
{
    /// <summary>Newest first; <paramref name="take"/> may return one extra row so the caller knows there is more.</summary>
    Task<IReadOnlyList<AuditEntry>> FindAsync(string? tableName, string? rowId, int skip, int take);
}
