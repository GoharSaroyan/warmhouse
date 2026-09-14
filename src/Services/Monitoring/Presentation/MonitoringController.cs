using Microsoft.AspNetCore.Mvc;
using Monitoring.Api.Application;

namespace Monitoring.Api.Presentation;

/// <summary>Reached via the API Gateway at /api/v1/monitoring.</summary>
[ApiController]
[Route("")]
public class MonitoringController : ControllerBase
{
    private readonly MonitoringHandler _handler;

    public MonitoringController(MonitoringHandler handler) => _handler = handler;

    // GET /api/v1/monitoring?houseId=... - everything currently visible for a house.
    [HttpGet]
    public async Task<ActionResult<List<LiveStateResponse>>> GetForHouse([FromQuery] Guid houseId, CancellationToken ct)
    {
        if (houseId == Guid.Empty)
        {
            return BadRequest(new { error = "houseId is required" });
        }

        var states = await _handler.GetForHouseAsync(houseId, ct);
        return Ok(states.Select(LiveStateResponse.From));
    }

    // GET /api/v1/monitoring/{deviceId}
    [HttpGet("{deviceId:guid}")]
    public async Task<ActionResult<LiveStateResponse>> Get(Guid deviceId, CancellationToken ct)
    {
        var state = await _handler.GetAsync(deviceId, ct);
        return state is null ? NotFound(new { error = "No live state for this device" }) : Ok(LiveStateResponse.From(state));
    }

    // POST /api/v1/monitoring/{deviceId}/report - manual/backfill path,
    // sharing the same MonitoringHandler.ReportAsync that
    // DeviceStateChangedConsumer calls for the normal, broker-driven path.
    [HttpPost("{deviceId:guid}/report")]
    public async Task<ActionResult<LiveStateResponse>> Report(Guid deviceId, [FromBody] ReportStateRequest request, CancellationToken ct)
    {
        var state = await _handler.ReportAsync(deviceId, request, ct);
        return Ok(LiveStateResponse.From(state));
    }
}
