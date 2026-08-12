using CallCenter.Domain.Enums;

namespace CallCenter.Domain.Entities;

public class Call
{
    public int Id { get; set; }
    public Guid CorrelationId { get; set; } = Guid.NewGuid();

    public CallDirection Direction { get; set; }
    public CallStatus Status { get; set; } = CallStatus.Queued;

    public string FromNumber { get; set; } = string.Empty;
    public string ToNumber { get; set; } = string.Empty;

    public int? QueueId { get; set; }
    public Queue? Queue { get; set; }

    public int? AgentId { get; set; }
    public Agent? Agent { get; set; }

    // Resolved at routing time from the (mocked) CRM.
    public string? CrmContactId { get; set; }
    public string? CrmContactName { get; set; }

    public DateTime QueuedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ConnectedAtUtc { get; set; }
    public DateTime? EndedAtUtc { get; set; }

    public string? DispositionCode { get; set; }
    public string? Notes { get; set; }

    // Recording is represented as metadata only in this prototype -
    // no real audio is captured (see AI-Readiness / System Design docs).
    public string? RecordingReference { get; set; }
}
