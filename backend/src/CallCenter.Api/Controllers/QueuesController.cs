using CallCenter.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CallCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QueuesController : ControllerBase
{
    private readonly IQueueRepository _queues;

    public QueuesController(IQueueRepository queues) => _queues = queues;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var queues = await _queues.GetAllAsync(ct);
        return Ok(queues.Select(q => new { q.Id, q.Name, q.RequiredSkill }));
    }
}
