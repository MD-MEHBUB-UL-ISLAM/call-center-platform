using CallCenter.Application.Interfaces;
using CallCenter.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CallCenter.Infrastructure.ExternalServices;

/// <summary>
/// Stand-in for the CRM Integration Service's dependency on a real CRM API
/// (see 04-system-design.md, section 3.4 - "CRM integration isolated behind one service").
/// In a full build, this class is replaced by an HTTP client calling the real CRM;
/// nothing outside this class would need to change, since callers only depend on ICrmService.
/// </summary>
public class MockCrmService : ICrmService
{
    private readonly ILogger<MockCrmService> _logger;

    // In-memory "CRM" so the demo can show a real screen-pop for a known number
    // and a graceful "unknown caller" path for an unknown one.
    private static readonly Dictionary<string, CrmContact> Contacts = new()
    {
        ["+8801700000001"] = new CrmContact
        {
            ContactId = "CRM-1001",
            FullName = "Tanvir Rahman",
            PhoneNumber = "+8801700000001",
            Company = "Green Valley Traders",
            Tier = "Gold",
            LastInteractionSummary = "Asked about bulk honey pricing last week; awaiting quote."
        },
        ["+8801700000002"] = new CrmContact
        {
            ContactId = "CRM-1002",
            FullName = "Farzana Akter",
            PhoneNumber = "+8801700000002",
            Company = "Akter Retail",
            Tier = "Standard",
            LastInteractionSummary = "Reported a delayed delivery two days ago; case still open."
        }
    };

    public MockCrmService(ILogger<MockCrmService> logger)
    {
        _logger = logger;
    }

    public Task<CrmContact?> LookupByPhoneNumberAsync(string phoneNumber, CancellationToken ct = default)
    {
        Contacts.TryGetValue(phoneNumber, out var contact);
        return Task.FromResult(contact);
    }

    public Task WriteCallOutcomeAsync(string contactId, int callId, string dispositionCode, string? notes, CancellationToken ct = default)
    {
        // A real implementation would POST/PATCH to the CRM's API here, with retry/queue
        // handling as described in the Scalability Plan (CRM API rate limits).
        _logger.LogInformation(
            "CRM write-back (mock): contact={ContactId} call={CallId} disposition={Disposition} notes={Notes}",
            contactId, callId, dispositionCode, notes);
        return Task.CompletedTask;
    }
}
