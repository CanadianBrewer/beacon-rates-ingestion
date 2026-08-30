using DueDiligenceWorks.Beacon.RateIngestion.Services;
using Microsoft.AspNetCore.Mvc;

namespace DueDiligenceWorks.Beacon.RateIngestion.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthCheckController(IHealthCheckService service): ControllerBase
{
    [HttpGet(Name = "PerformHealthCheck")]
    public async Task<IActionResult> PerformHealthCheck()
    {
        bool success = await service.PerformHealthCheck();
        return Ok(success);
    }
}