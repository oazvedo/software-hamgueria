using Microsoft.EntityFrameworkCore;
using SoftwareHamgueria.Application.Repositories;
using SoftwareHamgueria.Domain.Entities;
using SoftwareHamgueria.Infrastructure.Data;

namespace SoftwareHamgueria.Infrastructure.Repositories;

public class AuditRepository(AppDbContext context) : IAuditRepository
{
    public async Task<IReadOnlyList<AuditEntry>> FindAsync(string? tableName, string? rowId, int skip, int take)
    {
        var query = context.AuditLog.AsNoTracking();
        if (tableName is not null) query = query.Where(e => e.TableName == tableName);
        if (rowId is not null) query = query.Where(e => e.RowId == rowId);
        return await query.OrderByDescending(e => e.Id).Skip(skip).Take(take).ToListAsync();
    }
}
