using CallCenter.Domain.Enums;

namespace CallCenter.Application.DTOs;

public record AgentDto(int Id, string Name, string Email, UserRole Role, AgentStatus Status, string? QueueName, List<string> Skills);

public record UpdateAgentStatusRequest(AgentStatus Status);

public record LoginRequest(string Email, string Password);

public record LoginResponse(string Token, AgentDto Agent);
