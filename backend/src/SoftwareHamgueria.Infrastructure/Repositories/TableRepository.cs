using Microsoft.EntityFrameworkCore;
using SoftwareHamgueria.Application.Repositories;
using SoftwareHamgueria.Domain.Entities;
using SoftwareHamgueria.Infrastructure.Data;

namespace SoftwareHamgueria.Infrastructure.Repositories;

public class TableRepository(AppDbContext context) : ITableRepository
{
    public async Task AddAsync(Table table)
    {
        await context.Tables.AddAsync(table);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Table table)
    {
        context.Tables.Update(table);
        await context.SaveChangesAsync();
    }

    public Task<Table?> GetByIdAsync(Guid id) =>
        context.Tables.FirstOrDefaultAsync(t => t.Id == id);

    public async Task<IEnumerable<Table>> GetAllAsync() =>
        await context.Tables.ToListAsync();
}
