using BCrypt.Net;
using CallCenter.Domain.Entities;
using CallCenter.Domain.Enums;

namespace CallCenter.Infrastructure.Persistence;

/// <summary>Seeds demo data so the prototype is immediately usable without a setup script.</summary>
public static class DbSeeder
{
    public static void Seed(CallCenterDbContext db)
    {
        if (db.Queues.Any()) return;

        var salesQueue = new Queue { Name = "Sales", RequiredSkill = "sales" };
        var supportQueue = new Queue { Name = "Support", RequiredSkill = "support" };
        db.Queues.AddRange(salesQueue, supportQueue);
        db.SaveChanges();

        var agents = new List<Agent>
        {
            new()
            {
                Name = "Rafi Ahmed",
                Email = "rafi.agent@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                Role = UserRole.Agent,
                Status = AgentStatus.Available,
                QueueId = supportQueue.Id,
                Skills = new List<string> { "support" }
            },
            new()
            {
                Name = "Nusrat Jahan",
                Email = "nusrat.agent@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                Role = UserRole.Agent,
                Status = AgentStatus.Available,
                QueueId = salesQueue.Id,
                Skills = new List<string> { "sales" }
            },
            new()
            {
                Name = "Imran Hossain",
                Email = "imran.supervisor@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                Role = UserRole.Supervisor,
                Status = AgentStatus.Offline,
                Skills = new List<string>()
            }
        };

        db.Agents.AddRange(agents);
        db.SaveChanges();
    }
}
