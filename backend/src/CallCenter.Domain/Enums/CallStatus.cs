namespace CallCenter.Domain.Enums;

public enum CallStatus
{
    Queued = 0,
    Ringing = 1,
    Connected = 2,
    OnHold = 3,
    Completed = 4,
    Abandoned = 5
}
