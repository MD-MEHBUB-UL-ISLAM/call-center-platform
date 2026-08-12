namespace CallCenter.Application.DTOs;

public record CallVolumeReport(
    int TotalCalls,
    int CompletedCalls,
    int AbandonedCalls,
    double AverageHandleTimeSeconds,
    double AverageWaitTimeSeconds);

public record AgentProductivityReport(int AgentId, string AgentName, int CallsHandled, double AverageHandleTimeSeconds);
