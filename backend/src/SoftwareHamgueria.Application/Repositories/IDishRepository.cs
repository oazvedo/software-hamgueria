using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.Application.Repositories;

public interface IDishRepository
{
    Task AddAsync(Dish dish);
    Task UpdateAsync(Dish dish);
    Task<Dish?> GetByIdAsync(Guid id);
    Task<IEnumerable<Dish>> GetAllAsync(bool onlyActive = false);
}
