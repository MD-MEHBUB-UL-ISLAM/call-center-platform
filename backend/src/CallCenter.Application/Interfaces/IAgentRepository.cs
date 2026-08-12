using CallCenter.Domain.Entities;

namespace CallCenter.Application.Interfaces;

public interface IAgentRepository
{
    Task<Agent?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Agent?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<List<Agent>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Available agents attached to the given queue, ordered for routing selection.</summary>
    Task<List<Agent>> GetAvailableAgentsForQueueAsync(int queueId, CancellationToken ct = default);

    Task UpdateAsync(Agent agent, CancellationToken ct = default);
}
