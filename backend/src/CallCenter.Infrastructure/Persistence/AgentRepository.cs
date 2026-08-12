using CallCenter.Application.Interfaces;
using CallCenter.Domain.Entities;
using CallCenter.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CallCenter.Infrastructure.Persistence;

public class AgentRepository : IAgentRepository
{
    private readonly CallCenterDbContext _db;

    public AgentRepository(CallCenterDbContext db) => _db = db;

    public Task<Agent?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Agents.Include(a => a.Queue).FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<Agent?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        _db.Agents.Include(a => a.Queue).FirstOrDefaultAsync(a => a.Email == email, ct);

    public Task<List<Agent>> GetAllAsync(CancellationToken ct = default) =>
        _db.Agents.Include(a => a.Queue).AsNoTracking().ToListAsync(ct);

    public Task<List<Agent>> GetAvailableAgentsForQueueAsync(int queueId, CancellationToken ct = default) =>
        _db.Agents
            .Where(a => a.QueueId == queueId && a.Status == AgentStatus.Available)
            .OrderBy(a => a.Id) // stand-in for "longest idle" ordering in a full build
            .ToListAsync(ct);

    public async Task UpdateAsync(Agent agent, CancellationToken ct = default)
    {
        _db.Agents.Update(agent);
        await _db.SaveChangesAsync(ct);
    }
}
