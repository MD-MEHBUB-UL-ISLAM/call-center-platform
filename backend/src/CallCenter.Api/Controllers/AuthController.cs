using CallCenter.Application.DTOs;
using CallCenter.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CallCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAgentRepository _agents;
    private readonly ITokenService _tokens;

    public AuthController(IAgentRepository agents, ITokenService tokens)
    {
        _agents = agents;
        _tokens = tokens;
    }

    /// <summary>Demo login - validates against the seeded agent list. Not production auth.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var agent = await _agents.GetByEmailAsync(request.Email, ct);

        if (agent is null || !BCrypt.Net.BCrypt.Verify(request.Password, agent.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var token = _tokens.GenerateToken(agent);

        var dto = new AgentDto(agent.Id, agent.Name, agent.Email, agent.Role, agent.Status, agent.Queue?.Name, agent.Skills);
        return Ok(new LoginResponse(token, dto));
    }
}
