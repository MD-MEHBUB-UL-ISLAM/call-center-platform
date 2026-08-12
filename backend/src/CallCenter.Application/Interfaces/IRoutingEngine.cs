using CallCenter.Domain.Entities;

namespace CallCenter.Application.Interfaces;

/// <summary>Mirrors the "Routing Engine" component in the System Design doc.</summary>
public interface IRoutingEngine
{
    /// <summary>Picks the best available agent in a queue for an incoming call. Returns null if none are free (call stays queued).</summary>
    Task<Agent?> SelectAgentAsync(Queue queue, CancellationToken ct = default);
}
