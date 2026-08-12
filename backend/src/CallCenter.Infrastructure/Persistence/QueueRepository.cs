using CallCenter.Application.Interfaces;
using CallCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CallCenter.Infrastructure.Persistence;

public class QueueRepository : IQueueRepository
{
    private readonly CallCenterDbContext _db;

    public QueueRepository(CallCenterDbContext db) => _db = db;

    public Task<Queue?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Queues.FirstOrDefaultAsync(q => q.Id == id, ct);

    public Task<Queue?> GetByNameAsync(string name, CancellationToken ct = default) =>
        _db.Queues.FirstOrDefaultAsync(q => q.Name == name, ct);

    public Task<List<Queue>> GetAllAsync(CancellationToken ct = default) =>
        _db.Queues.AsNoTracking().ToListAsync(ct);
}
