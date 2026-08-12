using CallCenter.Application.Interfaces;
using CallCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CallCenter.Infrastructure.Persistence;

public class CallRepository : ICallRepository
{
    private readonly CallCenterDbContext _db;

    public CallRepository(CallCenterDbContext db) => _db = db;

    public Task<Call?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Calls.Include(c => c.Agent).Include(c => c.Queue).FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<List<Call>> GetRecentAsync(int take = 50, CancellationToken ct = default) =>
        _db.Calls.Include(c => c.Agent).Include(c => c.Queue)
            .OrderByDescending(c => c.QueuedAtUtc)
            .Take(take)
            .AsNoTracking()
            .ToListAsync(ct);

    public Task<List<Call>> GetForAgentAsync(int agentId, CancellationToken ct = default) =>
        _db.Calls.Include(c => c.Queue)
            .Where(c => c.AgentId == agentId)
            .OrderByDescending(c => c.QueuedAtUtc)
            .AsNoTracking()
            .ToListAsync(ct);

    public Task<List<Call>> GetAllAsync(CancellationToken ct = default) =>
        _db.Calls.Include(c => c.Agent).AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(Call call, CancellationToken ct = default)
    {
        _db.Calls.Add(call);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Call call, CancellationToken ct = default)
    {
        _db.Calls.Update(call);
        await _db.SaveChangesAsync(ct);
    }
}
