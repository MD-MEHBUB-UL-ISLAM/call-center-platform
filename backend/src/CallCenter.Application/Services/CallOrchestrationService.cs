using CallCenter.Application.DTOs;
using CallCenter.Application.Interfaces;
using CallCenter.Domain.Entities;
using CallCenter.Domain.Enums;

namespace CallCenter.Application.Services;

/// <summary>
/// Coordinates the inbound/outbound call lifecycle across routing, the (mocked) telephony
/// layer, CRM lookup/write-back, and real-time notifications. This is the prototype's stand-in
/// for the combination of "Telephony Orchestration Service" + "Call Session Service" described
/// in 04-system-design.md - collapsed into one class here for a demoable scope, with clear
/// seams (IRoutingEngine, ICrmService, ICallNotifier) showing where each would split out in
/// a full build.
/// </summary>
public class CallOrchestrationService
{
    private readonly ICallRepository _callRepository;
    private readonly IQueueRepository _queueRepository;
    private readonly IAgentRepository _agentRepository;
    private readonly IRoutingEngine _routingEngine;
    private readonly ICrmService _crmService;
    private readonly ICallNotifier _notifier;

    public CallOrchestrationService(
        ICallRepository callRepository,
        IQueueRepository queueRepository,
        IAgentRepository agentRepository,
        IRoutingEngine routingEngine,
        ICrmService crmService,
        ICallNotifier notifier)
    {
        _callRepository = callRepository;
        _queueRepository = queueRepository;
        _agentRepository = agentRepository;
        _routingEngine = routingEngine;
        _crmService = crmService;
        _notifier = notifier;
    }

    /// <summary>
    /// Simulates a call arriving from the SIP trunk / CPaaS provider (see System Design,
    /// section 3.1 on buying the telephony core). In a full build, this method would be
    /// triggered by a webhook/event from that provider instead of an API call.
    /// </summary>
    public async Task<CallDto> HandleInboundCallAsync(SimulateInboundCallRequest request, CancellationToken ct = default)
    {
        var queue = await _queueRepository.GetByNameAsync(request.QueueName, ct)
            ?? throw new InvalidOperationException($"Queue '{request.QueueName}' was not found.");

        var contact = await _crmService.LookupByPhoneNumberAsync(request.FromNumber, ct);

        var call = new Call
        {
            Direction = CallDirection.Inbound,
            Status = CallStatus.Queued,
            FromNumber = request.FromNumber,
            ToNumber = request.ToNumber,
            QueueId = queue.Id,
            CrmContactId = contact?.ContactId,
            CrmContactName = contact?.FullName,
            QueuedAtUtc = DateTime.UtcNow
        };

        await _callRepository.AddAsync(call, ct);

        var agent = await _routingEngine.SelectAgentAsync(queue, ct);

        if (agent is null)
        {
            // No free agent right now - call stays in Queued state.
            // A full build would push this into the live queue-depth SignalR broadcast.
            await _notifier.NotifyQueueUpdatedAsync(queue.Name, 1, ct);
            return ToDto(call, queue.Name, null);
        }

        call.Status = CallStatus.Ringing;
        call.AgentId = agent.Id;
        await _callRepository.UpdateAsync(call, ct);

        agent.Status = AgentStatus.OnCall;
        await _agentRepository.UpdateAsync(agent, ct);

        await _notifier.NotifyIncomingCallAsync(
            agent.Id,
            new IncomingCallNotification(
                call.Id,
                call.CorrelationId,
                call.FromNumber,
                contact?.ContactId,
                contact?.FullName,
                contact?.Company,
                contact?.Tier,
                contact?.LastInteractionSummary,
                queue.Name),
            ct);

        return ToDto(call, queue.Name, agent.Name);
    }

    /// <summary>Agent accepts a ringing call - bridges it (simulated).</summary>
    public async Task<CallDto> AcceptCallAsync(int callId, int agentId, CancellationToken ct = default)
    {
        var call = await _callRepository.GetByIdAsync(callId, ct)
            ?? throw new InvalidOperationException("Call not found.");

        if (call.AgentId != agentId)
            throw new InvalidOperationException("This call is not assigned to the requesting agent.");

        call.Status = CallStatus.Connected;
        call.ConnectedAtUtc = DateTime.UtcNow;
        await _callRepository.UpdateAsync(call, ct);

        var queueName = (await _queueRepository.GetByIdAsync(call.QueueId ?? 0, ct))?.Name;
        return ToDto(call, queueName, (await _agentRepository.GetByIdAsync(agentId, ct))?.Name);
    }

    /// <summary>Places an outbound call on behalf of an agent (e.g. click-to-call from the CRM).</summary>
    public async Task<CallDto> PlaceOutboundCallAsync(int agentId, PlaceOutboundCallRequest request, CancellationToken ct = default)
    {
        var agent = await _agentRepository.GetByIdAsync(agentId, ct)
            ?? throw new InvalidOperationException("Agent not found.");

        var contact = await _crmService.LookupByPhoneNumberAsync(request.ToNumber, ct);

        var call = new Call
        {
            Direction = CallDirection.Outbound,
            Status = CallStatus.Connected,
            FromNumber = "internal",
            ToNumber = request.ToNumber,
            AgentId = agent.Id,
            CrmContactId = contact?.ContactId,
            CrmContactName = contact?.FullName,
            QueuedAtUtc = DateTime.UtcNow,
            ConnectedAtUtc = DateTime.UtcNow
        };

        await _callRepository.AddAsync(call, ct);

        agent.Status = AgentStatus.OnCall;
        await _agentRepository.UpdateAsync(agent, ct);

        return ToDto(call, null, agent.Name);
    }

    /// <summary>
    /// Ends a call: records disposition/notes, writes the outcome back to the CRM, frees the
    /// agent, and notifies the client - mirroring the CallEnded event fan-out described in the
    /// System Design and AI-Readiness docs (recording finalization / reporting would be
    /// additional consumers of that same event in a full build).
    /// </summary>
    public async Task<CallDto> EndCallAsync(int callId, int agentId, EndCallRequest request, CancellationToken ct = default)
    {
        var call = await _callRepository.GetByIdAsync(callId, ct)
            ?? throw new InvalidOperationException("Call not found.");

        call.Status = CallStatus.Completed;
        call.EndedAtUtc = DateTime.UtcNow;
        call.DispositionCode = request.DispositionCode;
        call.Notes = request.Notes;
        call.RecordingReference = $"rec-{call.CorrelationId}"; // metadata placeholder only
        await _callRepository.UpdateAsync(call, ct);

        if (!string.IsNullOrEmpty(call.CrmContactId))
        {
            await _crmService.WriteCallOutcomeAsync(call.CrmContactId, call.Id, request.DispositionCode, request.Notes, ct);
        }

        var agent = await _agentRepository.GetByIdAsync(agentId, ct);
        if (agent is not null)
        {
            agent.Status = AgentStatus.Available;
            await _agentRepository.UpdateAsync(agent, ct);
        }

        var queueName = call.QueueId.HasValue ? (await _queueRepository.GetByIdAsync(call.QueueId.Value, ct))?.Name : null;
        var dto = ToDto(call, queueName, agent?.Name);

        await _notifier.NotifyCallEndedAsync(agentId, dto, ct);

        return dto;
    }

    private static CallDto ToDto(Call call, string? queueName, string? agentName) => new(
        call.Id,
        call.CorrelationId,
        call.Direction,
        call.Status,
        call.FromNumber,
        call.ToNumber,
        call.AgentId,
        agentName,
        queueName,
        call.CrmContactId,
        call.CrmContactName,
        call.QueuedAtUtc,
        call.ConnectedAtUtc,
        call.EndedAtUtc,
        call.DispositionCode,
        call.Notes);
}
