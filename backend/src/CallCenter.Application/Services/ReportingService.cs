using CallCenter.Application.DTOs;
using CallCenter.Application.Interfaces;
using CallCenter.Domain.Enums;

namespace CallCenter.Application.Services;

/// <summary>Prototype of the Reporting Service in 04-system-design.md - computed on demand from stored calls.</summary>
public class ReportingService
{
    private readonly ICallRepository _callRepository;

    public ReportingService(ICallRepository callRepository)
    {
        _callRepository = callRepository;
    }

    public async Task<CallVolumeReport> GetCallVolumeReportAsync(CancellationToken ct = default)
    {
        var calls = await _callRepository.GetAllAsync(ct);

        var completed = calls.Where(c => c.Status == CallStatus.Completed).ToList();
        var abandoned = calls.Count(c => c.Status == CallStatus.Abandoned);

        var handleTimes = completed
            .Where(c => c.ConnectedAtUtc.HasValue && c.EndedAtUtc.HasValue)
            .Select(c => (c.EndedAtUtc!.Value - c.ConnectedAtUtc!.Value).TotalSeconds)
            .ToList();

        var waitTimes = completed
            .Where(c => c.ConnectedAtUtc.HasValue)
            .Select(c => (c.ConnectedAtUtc!.Value - c.QueuedAtUtc).TotalSeconds)
            .ToList();

        return new CallVolumeReport(
            TotalCalls: calls.Count,
            CompletedCalls: completed.Count,
            AbandonedCalls: abandoned,
            AverageHandleTimeSeconds: handleTimes.Count > 0 ? Math.Round(handleTimes.Average(), 1) : 0,
            AverageWaitTimeSeconds: waitTimes.Count > 0 ? Math.Round(waitTimes.Average(), 1) : 0);
    }

    public async Task<List<AgentProductivityReport>> GetAgentProductivityAsync(CancellationToken ct = default)
    {
        var calls = await _callRepository.GetAllAsync(ct);

        return calls
            .Where(c => c.Status == CallStatus.Completed && c.AgentId.HasValue)
            .GroupBy(c => new { c.AgentId, AgentName = c.Agent?.Name ?? "Unknown" })
            .Select(g => new AgentProductivityReport(
                g.Key.AgentId!.Value,
                g.Key.AgentName,
                g.Count(),
                g.Where(c => c.ConnectedAtUtc.HasValue && c.EndedAtUtc.HasValue)
                 .Select(c => (c.EndedAtUtc!.Value - c.ConnectedAtUtc!.Value).TotalSeconds)
                 .DefaultIfEmpty(0)
                 .Average()))
            .ToList();
    }
}
