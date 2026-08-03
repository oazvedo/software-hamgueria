using Microsoft.EntityFrameworkCore;
using SoftwareHamgueria.Application.Repositories;
using SoftwareHamgueria.Domain.Entities;
using SoftwareHamgueria.Infrastructure.Data;

namespace SoftwareHamgueria.Infrastructure.Repositories;

public class ComandaRepository(AppDbContext context) : IComandaRepository
{
    public Task<ComandaClosure?> GetAsync(Guid tableId, string customerName) =>
        context.ComandaClosures.FirstOrDefaultAsync(
            c => c.TableId == tableId && c.CustomerName.ToLower() == customerName.ToLower());

    public async Task<IEnumerable<ComandaClosure>> GetForTableAsync(Guid tableId) =>
        await context.ComandaClosures.Where(c => c.TableId == tableId).ToListAsync();

    public async Task<IEnumerable<ComandaClosure>> GetAllAsync() =>
        await context.ComandaClosures.ToListAsync();

    public async Task UpsertAsync(Guid tableId, string customerName)
    {
        var existing = await context.ComandaClosures.FirstOrDefaultAsync(
            c => c.TableId == tableId && c.CustomerName.ToLower() == customerName.ToLower());
        if (existing is null)
            context.ComandaClosures.Add(new ComandaClosure(tableId, customerName));
        else
            existing.Touch();
        await context.SaveChangesAsync();
    }
}
