using CallCenter.Application.DTOs;

namespace CallCenter.Application.Interfaces;

/// <summary>
/// Abstraction over the real-time push channel (SignalR in this prototype - see
/// CallCenter.Api.Hubs.CallHub / CallHubNotifier). Keeps the Application layer
/// free of any transport-specific (SignalR) dependency, matching the Notification
/// Hub component in the System Design doc.
/// </summary>
public interface ICallNotifier
{
    Task NotifyIncomingCallAsync(int agentId, IncomingCallNotification notification, CancellationToken ct = default);
    Task NotifyQueueUpdatedAsync(string queueName, int queueDepth, CancellationToken ct = default);
    Task NotifyAgentStatusChangedAsync(int agentId, string status, CancellationToken ct = default);
    Task NotifyCallEndedAsync(int agentId, CallDto call, CancellationToken ct = default);
}
