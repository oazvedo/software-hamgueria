using FluentValidation;
using SoftwareHamgueria.Application.DTOs.Audit;
using SoftwareHamgueria.Application.Repositories;
using SoftwareHamgueria.Application.Services.Interfaces;
using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.Application.Services;

public class AuditService(IAuditRepository repository, IValidator<AuditQuery> validator) : IAuditService
{
    public async Task<AuditPageDto> GetAsync(AuditQuery query)
    {
        await validator.ValidateAndThrowAsync(query);
        var rows = await repository.FindAsync(query.Table, query.RowId, (query.Page - 1) * query.PageSize, query.PageSize + 1);
        var hasMore = rows.Count > query.PageSize;
        return new AuditPageDto(rows.Take(query.PageSize).Select(ToDto).ToList(), query.Page, query.PageSize, hasMore);
    }

    private static AuditEntryDto ToDto(AuditEntry e) => new(
        e.Id, e.TableName, e.Operation, e.RowId, e.ChangedFields, e.OldValues, e.NewValues,
        string.IsNullOrWhiteSpace(e.AuthorName) ? "anônimo" : e.AuthorName, e.FrontendUrl, e.RequestId, e.CreatedAt);
}
