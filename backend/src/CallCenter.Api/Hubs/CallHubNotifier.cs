using CallCenter.Application.DTOs;
using CallCenter.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace CallCenter.Api.Hubs;

/// <summary>SignalR-backed implementation of ICallNotifier - the transport-specific piece the Application layer doesn't know about.</summary>
public class CallHubNotifier : ICallNotifier
{
    private readonly IHubContext<CallHub> _hub;

    public CallHubNotifier(IHubContext<CallHub> hub) => _hub = hub;

    public Task NotifyIncomingCallAsync(int agentId, IncomingCallNotification notification, CancellationToken ct = default) =>
        _hub.Clients.Group($"agent-{agentId}").SendAsync("IncomingCall", notification, ct);

    public Task NotifyQueueUpdatedAsync(string queueName, int queueDepth, CancellationToken ct = default) =>
        _hub.Clients.All.SendAsync("QueueUpdated", new { queueName, queueDepth }, ct);

    public Task NotifyAgentStatusChangedAsync(int agentId, string status, CancellationToken ct = default) =>
        _hub.Clients.All.SendAsync("AgentStatusChanged", new { agentId, status }, ct);

    public Task NotifyCallEndedAsync(int agentId, CallDto call, CancellationToken ct = default) =>
        _hub.Clients.Group($"agent-{agentId}").SendAsync("CallEnded", call, ct);
}
