using Lighting.Api.Application;
using Microsoft.AspNetCore.Mvc;

namespace Lighting.Api.Presentation;

/// <summary>Reached via the API Gateway at /api/v1/lighting.</summary>
[ApiController]
[Route("")]
public class LightingController : ControllerBase
{
    private readonly LightingCommandHandler _handler;

    public LightingController(LightingCommandHandler handler) => _handler = handler;

    [HttpGet("{deviceId:guid}")]
    public async Task<ActionResult<LightingStateResponse>> Get(Guid deviceId, CancellationToken ct)
    {
        var state = await _handler.GetStateAsync(deviceId, ct);
        return state is null ? NotFound(new { error = "No lighting state for this device" }) : Ok(LightingStateResponse.From(state));
    }

    [HttpPut("{deviceId:guid}")]
    public async Task<ActionResult<LightingStateResponse>> SetDesired(Guid deviceId, [FromBody] SetLightingRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.DesiredValue))
        {
            return BadRequest(new { error = "desiredValue is required" });
        }

        var state = await _handler.SetDesiredAsync(deviceId, request.DesiredValue, ct);
        return Ok(LightingStateResponse.From(state));
    }

    // POST /api/v1/lighting/{deviceId}/ack - stands in for the command-ack
    // event that would arrive from the Message Broker once it exists.
    [HttpPost("{deviceId:guid}/ack")]
    public async Task<ActionResult<LightingStateResponse>> ReportActual(Guid deviceId, [FromBody] SetLightingRequest request, CancellationToken ct)
    {
        var state = await _handler.ReportActualAsync(deviceId, request.DesiredValue, ct);
        return state is null ? NotFound(new { error = "No lighting state for this device" }) : Ok(LightingStateResponse.From(state));
    }
}
