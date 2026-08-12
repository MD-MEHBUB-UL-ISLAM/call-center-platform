namespace CallCenter.Domain.Entities;

/// <summary>
/// Represents a customer record as it would be resolved from the company CRM.
/// In this prototype it is served by a mocked CRM client instead of a live CRM API
/// (see CallCenter.Infrastructure.ExternalServices.MockCrmService).
/// </summary>
public class CrmContact
{
    public string ContactId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Tier { get; set; } = "Standard";
    public string LastInteractionSummary { get; set; } = string.Empty;
}
