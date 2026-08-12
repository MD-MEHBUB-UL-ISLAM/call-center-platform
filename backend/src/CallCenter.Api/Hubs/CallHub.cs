using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CallCenter.Api.Hubs;

/// <summary>
/// Real-time push channel to agent/supervisor clients - the "Notification Hub" component
/// in 04-system-design.md. Clients join a group named "agent-{agentId}" on connect so
/// notifications can be targeted to the right agent without a broadcast to everyone
/// (see 05-scalability-plan.md, section 4, on SignalR fan-out).
/// </summary>
[Authorize]
public class CallHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var agentId = Context.User?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        if (!string.IsNullOrEmpty(agentId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"agent-{agentId}");
        }

        await base.OnConnectedAsync();
    }
}
