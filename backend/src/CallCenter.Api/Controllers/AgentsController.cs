using System.Security.Claims;
using CallCenter.Application.DTOs;
using CallCenter.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CallCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgentsController : ControllerBase
{
    private readonly IAgentRepository _agents;
    private readonly ICallNotifier _notifier;

    public AgentsController(IAgentRepository agents, ICallNotifier notifier)
    {
        _agents = agents;
        _notifier = notifier;
    }

    [HttpGet]
    public async Task<ActionResult<List<AgentDto>>> GetAll(CancellationToken ct)
    {
        var agents = await _agents.GetAllAsync(ct);
        return Ok(agents.Select(a => new AgentDto(a.Id, a.Name, a.Email, a.Role, a.Status, a.Queue?.Name, a.Skills)));
    }

    [HttpGet("me")]
    public async Task<ActionResult<AgentDto>> GetMe(CancellationToken ct)
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
        var agent = await _agents.GetByIdAsync(id, ct);
        if (agent is null) return NotFound();

        return Ok(new AgentDto(agent.Id, agent.Name, agent.Email, agent.Role, agent.Status, agent.Queue?.Name, agent.Skills));
    }

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<AgentDto>> UpdateStatus(int id, UpdateAgentStatusRequest request, CancellationToken ct)
    {
        var agent = await _agents.GetByIdAsync(id, ct);
        if (agent is null) return NotFound();

        agent.Status = request.Status;
        await _agents.UpdateAsync(agent, ct);
        await _notifier.NotifyAgentStatusChangedAsync(id, agent.Status.ToString(), ct);

        return Ok(new AgentDto(agent.Id, agent.Name, agent.Email, agent.Role, agent.Status, agent.Queue?.Name, agent.Skills));
    }
}
