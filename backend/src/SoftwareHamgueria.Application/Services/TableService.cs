using SoftwareHamgueria.Application.DTOs.Table;
using SoftwareHamgueria.Application.Repositories;
using SoftwareHamgueria.Application.Services.Interfaces;
using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.Application.Services;

public class TableService(ITableRepository repository) : ITableService
{
    public async Task<TableResponseDto> CreateAsync(CreateTableDto dto)
    {
        var table = new Table(dto.TableName);
        await repository.AddAsync(table);
        return ToDto(table);
    }

    public async Task<TableResponseDto> UpdateAsync(Guid id, UpdateTableDto dto)
    {
        var table = await repository.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Table {id} not found.");
        table.Update(dto.TableName);
        await repository.UpdateAsync(table);
        return ToDto(table);
    }

    public async Task<TableResponseDto?> GetByIdAsync(Guid id)
    {
        var table = await repository.GetByIdAsync(id);
        return table is null ? null : ToDto(table);
    }

    public async Task<IEnumerable<TableResponseDto>> GetAllAsync()
    {
        var tables = await repository.GetAllAsync();
        return tables.Select(ToDto);
    }

    public async Task DisableAsync(Guid id)
    {
        var table = await repository.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Table {id} not found.");
        table.Disable();
        await repository.UpdateAsync(table);
    }

    private static TableResponseDto ToDto(Table t) =>
        new(t.Id, t.TableName, t.IsActive, t.CreatedAt, t.UpdatedAt);
}
