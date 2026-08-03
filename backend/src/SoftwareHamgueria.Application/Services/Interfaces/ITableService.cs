using SoftwareHamgueria.Application.DTOs.Table;

namespace SoftwareHamgueria.Application.Services.Interfaces;

public interface ITableService
{
    Task<TableResponseDto> CreateAsync(CreateTableDto dto);
    Task<TableResponseDto> UpdateAsync(Guid id, UpdateTableDto dto);
    Task<TableResponseDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<TableResponseDto>> GetAllAsync();
    Task DisableAsync(Guid id);
}
