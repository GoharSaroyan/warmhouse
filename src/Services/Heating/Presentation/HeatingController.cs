using Heating.Api.Application;
using Microsoft.AspNetCore.Mvc;

namespace Heating.Api.Presentation;

/// <summary>Reached via the API Gateway at /api/v1/heating.</summary>
[ApiController]
[Route("")]
public class HeatingController : ControllerBase
{
    private readonly HeatingCommandHandler _handler;

    public HeatingController(HeatingCommandHandler handler) => _handler = handler;

    // GET /api/v1/heating/{deviceId}
    [HttpGet("{deviceId:guid}")]
    public async Task<ActionResult<HeatingStateResponse>> Get(Guid deviceId, CancellationToken ct)
    {
        var state = await _handler.GetStateAsync(deviceId, ct);
        return state is null ? NotFound(new { error = "No heating state for this device" }) : Ok(HeatingStateResponse.From(state));
    }

    // PUT /api/v1/heating/{deviceId} - set desired state (e.g. "on"/"off").
    [HttpPut("{deviceId:guid}")]
    public async Task<ActionResult<HeatingStateResponse>> SetDesired(Guid deviceId, [FromBody] SetHeatingRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.DesiredValue))
        {
            return BadRequest(new { error = "desiredValue is required" });
        }

        var state = await _handler.SetDesiredAsync(deviceId, request.DesiredValue, ct);
        return Ok(HeatingStateResponse.From(state));
    }

    // POST /api/v1/heating/{deviceId}/ack - stands in for the command-ack
    // event that would arrive from the Message Broker once it exists.
    [HttpPost("{deviceId:guid}/ack")]
    public async Task<ActionResult<HeatingStateResponse>> ReportActual(Guid deviceId, [FromBody] SetHeatingRequest request, CancellationToken ct)
    {
        var state = await _handler.ReportActualAsync(deviceId, request.DesiredValue, ct);
        return state is null ? NotFound(new { error = "No heating state for this device" }) : Ok(HeatingStateResponse.From(state));
    }
}
