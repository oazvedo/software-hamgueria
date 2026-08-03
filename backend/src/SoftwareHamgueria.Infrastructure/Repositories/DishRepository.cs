using Microsoft.EntityFrameworkCore;
using SoftwareHamgueria.Application.Repositories;
using SoftwareHamgueria.Domain.Entities;
using SoftwareHamgueria.Infrastructure.Data;

namespace SoftwareHamgueria.Infrastructure.Repositories;

public class DishRepository(AppDbContext context) : IDishRepository
{
    public async Task AddAsync(Dish dish)
    {
        await context.Dishes.AddAsync(dish);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Dish dish)
    {
        context.Dishes.Update(dish);
        await context.SaveChangesAsync();
    }

    public Task<Dish?> GetByIdAsync(Guid id) =>
        context.Dishes.FirstOrDefaultAsync(d => d.Id == id);

    public async Task<IEnumerable<Dish>> GetAllAsync() =>
        await context.Dishes.ToListAsync();
}
