using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.Application.Repositories;

public interface ITableRepository
{
    Task AddAsync(Table table);
    Task UpdateAsync(Table table);
    Task<Table?> GetByIdAsync(Guid id);
    Task<IEnumerable<Table>> GetAllAsync();
}
