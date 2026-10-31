using FluentValidation;
using SoftwareHamgueria.Application.DTOs.Table;
using SoftwareHamgueria.Application.Exceptions;
using SoftwareHamgueria.Application.Repositories;
using SoftwareHamgueria.Application.Services.Interfaces;
using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.Application.Services;

public class TableService(
    ITableRepository repository,
    IValidator<CreateTableDto> createValidator,
    IValidator<UpdateTableDto> updateValidator) : ITableService
{
    public async Task<TableResponseDto> CreateAsync(CreateTableDto dto)
    {
        await createValidator.ValidateAndThrowAsync(dto);
        var table = new Table(dto.TableName.Trim());
        await repository.AddAsync(table);
        return ToDto(table);
    }

    public async Task<TableResponseDto> UpdateAsync(Guid id, UpdateTableDto dto)
    {
        await updateValidator.ValidateAndThrowAsync(dto);
        var table = await GetOrThrowAsync(id);
        table.Update(dto.TableName.Trim());
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
        var table = await GetOrThrowAsync(id);
        table.Disable();
        await repository.UpdateAsync(table);
    }

    private async Task<Table> GetOrThrowAsync(Guid id) =>
        await repository.GetByIdAsync(id) ?? throw new NotFoundException("Mesa não encontrada.");

    private static TableResponseDto ToDto(Table t) =>
        new(t.Id, t.TableName, t.IsActive, t.CreatedAt, t.UpdatedAt);
}
