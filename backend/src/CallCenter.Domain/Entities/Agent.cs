using CallCenter.Domain.Enums;

namespace CallCenter.Domain.Entities;

public class Agent
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Demo-only credential; a real system would never store a plain password.
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Agent;
    public AgentStatus Status { get; set; } = AgentStatus.Offline;

    public int? QueueId { get; set; }
    public Queue? Queue { get; set; }

    public List<string> Skills { get; set; } = new();

    public ICollection<Call> Calls { get; set; } = new List<Call>();
}
