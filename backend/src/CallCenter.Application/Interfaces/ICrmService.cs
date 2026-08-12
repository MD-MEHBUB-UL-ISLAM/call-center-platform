using CallCenter.Domain.Entities;

namespace CallCenter.Application.Interfaces;

/// <summary>
/// Abstraction over the CRM Integration Service described in the System Design doc.
/// The prototype implementation (MockCrmService) returns fake data so the routing
/// and screen-pop flow can be demonstrated without a live CRM connection.
/// </summary>
public interface ICrmService
{
    Task<CrmContact?> LookupByPhoneNumberAsync(string phoneNumber, CancellationToken ct = default);
    Task WriteCallOutcomeAsync(string contactId, int callId, string dispositionCode, string? notes, CancellationToken ct = default);
}
