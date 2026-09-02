using DueDiligenceWorks.Beacon.RateIngestion.Services;
using Microsoft.AspNetCore.Mvc;

namespace DueDiligenceWorks.Beacon.RateIngestion.Controllers;

[ApiController]
[Route("[controller]")]
public class RateIngestionController(IBeaconRatesApiService service): ControllerBase
{
    [HttpGet(Name = "GetAllRates")]
    [Route("all")]
    public async Task<IActionResult> GetAllRates()
    {
        await service.GetAllRates();
        return Ok();
    }

    [HttpGet(Name = "GetFixedRates")]
    [Route("fixed")]
    public async Task<IActionResult> GetFixedRates()
    {
        await service.GetFixedRatesV2();
        return Ok();
    }

    [HttpGet(Name = "GetIndexesRates")]
    [Route("indexed")]
    public async Task<IActionResult> GetIndexedRates()
    {
        await service.GetIndexedRates();
        return Ok();
    }

    [HttpGet(Name = "GetRilaRates")]
    [Route("rila")]
    public async Task<IActionResult> GetRilaRates()
    {
        await service.GetRilaRates();
        return Ok();
    }
}