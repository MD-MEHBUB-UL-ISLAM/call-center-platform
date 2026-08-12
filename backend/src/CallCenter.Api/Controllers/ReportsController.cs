using CallCenter.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CallCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Supervisor,Admin")]
public class ReportsController : ControllerBase
{
    private readonly ReportingService _reporting;

    public ReportsController(ReportingService reporting) => _reporting = reporting;

    [HttpGet("call-volume")]
    public async Task<IActionResult> CallVolume(CancellationToken ct) => Ok(await _reporting.GetCallVolumeReportAsync(ct));

    [HttpGet("agent-productivity")]
    public async Task<IActionResult> AgentProductivity(CancellationToken ct) => Ok(await _reporting.GetAgentProductivityAsync(ct));
}
