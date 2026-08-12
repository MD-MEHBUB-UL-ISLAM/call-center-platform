using System.Security.Claims;
using CallCenter.Application.DTOs;
using CallCenter.Application.Interfaces;
using CallCenter.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CallCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CallsController : ControllerBase
{
    private readonly CallOrchestrationService _orchestration;
    private readonly ICallRepository _calls;

    public CallsController(CallOrchestrationService orchestration, ICallRepository calls)
    {
        _orchestration = orchestration;
        _calls = calls;
    }

    private int CurrentAgentId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    /// <summary>
    /// Stands in for the telephony provider's inbound-call webhook (see System Design 3.1).
    /// In the interview demo, this is the button that simulates "a customer is calling in".
    /// </summary>
    [HttpPost("simulate-inbound")]
    [AllowAnonymous] // simulates an external system (the SIP/CPaaS provider) calling in - no agent session yet
    public async Task<ActionResult<CallDto>> SimulateInbound(SimulateInboundCallRequest request, CancellationToken ct)
    {
        var call = await _orchestration.HandleInboundCallAsync(request, ct);
        return Ok(call);
    }

    [HttpPost("{id:int}/accept")]
    public async Task<ActionResult<CallDto>> Accept(int id, CancellationToken ct)
    {
        var call = await _orchestration.AcceptCallAsync(id, CurrentAgentId, ct);
        return Ok(call);
    }

    [HttpPost("outbound")]
    public async Task<ActionResult<CallDto>> PlaceOutbound(PlaceOutboundCallRequest request, CancellationToken ct)
    {
        var call = await _orchestration.PlaceOutboundCallAsync(CurrentAgentId, request, ct);
        return Ok(call);
    }

    [HttpPost("{id:int}/end")]
    public async Task<ActionResult<CallDto>> End(int id, EndCallRequest request, CancellationToken ct)
    {
        var call = await _orchestration.EndCallAsync(id, CurrentAgentId, request, ct);
        return Ok(call);
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var calls = await _calls.GetForAgentAsync(CurrentAgentId, ct);
        return Ok(calls.Select(c => new
        {
            c.Id,
            c.Direction,
            c.Status,
            c.FromNumber,
            c.ToNumber,
            c.CrmContactName,
            c.QueuedAtUtc,
            c.ConnectedAtUtc,
            c.EndedAtUtc,
            c.DispositionCode,
            c.Notes
        }));
    }

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent(CancellationToken ct)
    {
        var calls = await _calls.GetRecentAsync(50, ct);
        return Ok(calls.Select(c => new
        {
            c.Id,
            c.Direction,
            c.Status,
            c.FromNumber,
            c.ToNumber,
            AgentName = c.Agent?.Name,
            QueueName = c.Queue?.Name,
            c.CrmContactName,
            c.QueuedAtUtc,
            c.EndedAtUtc,
            c.DispositionCode
        }));
    }
}
