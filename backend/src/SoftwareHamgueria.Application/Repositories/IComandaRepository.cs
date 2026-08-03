using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.Application.Repositories;

public interface IComandaRepository
{
    Task<ComandaClosure?> GetAsync(Guid tableId, string customerName);
    Task<IEnumerable<ComandaClosure>> GetForTableAsync(Guid tableId);
    Task<IEnumerable<ComandaClosure>> GetAllAsync();
    Task UpsertAsync(Guid tableId, string customerName);
}
