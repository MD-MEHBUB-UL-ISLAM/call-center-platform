using CallCenter.Domain.Enums;

namespace CallCenter.Application.DTOs;

public record CallDto(
    int Id,
    Guid CorrelationId,
    CallDirection Direction,
    CallStatus Status,
    string FromNumber,
    string ToNumber,
    int? AgentId,
    string? AgentName,
    string? QueueName,
    string? CrmContactId,
    string? CrmContactName,
    DateTime QueuedAtUtc,
    DateTime? ConnectedAtUtc,
    DateTime? EndedAtUtc,
    string? DispositionCode,
    string? Notes);

/// <summary>Simulates an inbound call arriving from the telephony/SIP layer.</summary>
public record SimulateInboundCallRequest(string FromNumber, string ToNumber, string QueueName);

public record PlaceOutboundCallRequest(string ToNumber);

public record EndCallRequest(string DispositionCode, string? Notes);

/// <summary>What the agent's screen-pop panel receives when a call is routed to them.</summary>
public record IncomingCallNotification(
    int CallId,
    Guid CorrelationId,
    string FromNumber,
    string? CrmContactId,
    string? CrmContactName,
    string? CrmCompany,
    string? CrmTier,
    string? CrmLastInteractionSummary,
    string QueueName);
