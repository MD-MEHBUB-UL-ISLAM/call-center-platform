using CallCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CallCenter.Infrastructure.Persistence;

public class CallCenterDbContext : DbContext
{
    public CallCenterDbContext(DbContextOptions<CallCenterDbContext> options) : base(options) { }

    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<Queue> Queues => Set<Queue>();
    public DbSet<Call> Calls => Set<Call>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Agent>(e =>
        {
            e.HasIndex(a => a.Email).IsUnique();
            e.Property(a => a.Skills)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
        });

        modelBuilder.Entity<Queue>()
            .HasIndex(q => q.Name)
            .IsUnique();

        modelBuilder.Entity<Call>()
            .HasOne(c => c.Agent)
            .WithMany(a => a.Calls)
            .HasForeignKey(c => c.AgentId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Call>()
            .HasOne(c => c.Queue)
            .WithMany(q => q.Calls)
            .HasForeignKey(c => c.QueueId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
