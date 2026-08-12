using CallCenter.Domain.Entities;

namespace CallCenter.Application.Interfaces;

public interface ICallRepository
{
    Task<Call?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Call>> GetRecentAsync(int take = 50, CancellationToken ct = default);
    Task<List<Call>> GetForAgentAsync(int agentId, CancellationToken ct = default);
    Task AddAsync(Call call, CancellationToken ct = default);
    Task UpdateAsync(Call call, CancellationToken ct = default);
    Task<List<Call>> GetAllAsync(CancellationToken ct = default);
}
