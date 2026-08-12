using CallCenter.Domain.Entities;

namespace CallCenter.Application.Interfaces;

public interface IQueueRepository
{
    Task<Queue?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Queue?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<List<Queue>> GetAllAsync(CancellationToken ct = default);
}
