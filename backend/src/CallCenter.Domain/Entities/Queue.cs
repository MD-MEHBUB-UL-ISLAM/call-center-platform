namespace CallCenter.Domain.Entities;

public class Queue
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>Skill tag required to receive calls from this queue (skill-based routing).</summary>
    public string RequiredSkill { get; set; } = string.Empty;

    public ICollection<Agent> Agents { get; set; } = new List<Agent>();
    public ICollection<Call> Calls { get; set; } = new List<Call>();
}
