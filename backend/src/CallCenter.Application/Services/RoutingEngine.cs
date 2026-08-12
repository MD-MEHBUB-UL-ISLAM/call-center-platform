using CallCenter.Application.Interfaces;
using CallCenter.Domain.Entities;
using CallCenter.Domain.Enums;

namespace CallCenter.Application.Services;

/// <summary>
/// Skill/queue-based routing, as scoped for MVP in 03-mvp-definition.md.
/// Strategy: first available agent attached to the queue, ordered by who has been
/// idle the longest is a natural v1.1 improvement - the prototype keeps it simple
/// (first available agent returned by the repository) to keep the demo focused on
/// proving the routing seam works end-to-end.
/// </summary>
public class RoutingEngine : IRoutingEngine
{
    private readonly IAgentRepository _agentRepository;

    public RoutingEngine(IAgentRepository agentRepository)
    {
        _agentRepository = agentRepository;
    }

    public async Task<Agent?> SelectAgentAsync(Queue queue, CancellationToken ct = default)
    {
        var candidates = await _agentRepository.GetAvailableAgentsForQueueAsync(queue.Id, ct);

        return candidates
            .Where(a => a.Status == AgentStatus.Available)
            .FirstOrDefault();
    }
}
