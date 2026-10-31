using SoftwareHamgueria.Application.DTOs.Audit;

namespace SoftwareHamgueria.Application.Services.Interfaces;

public interface IAuditService
{
    Task<AuditPageDto> GetAsync(AuditQuery query);
}
