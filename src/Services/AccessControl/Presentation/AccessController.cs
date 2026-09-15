using AccessControl.Api.Application;
using Microsoft.AspNetCore.Mvc;

namespace AccessControl.Api.Presentation;

/// <summary>Reached via the API Gateway at /api/v1/access.</summary>
[ApiController]
[Route("")]
public class AccessController : ControllerBase
{
    private readonly AccessCommandHandler _handler;

    public AccessController(AccessCommandHandler handler) => _handler = handler;

    [HttpGet("{deviceId:guid}")]
    public async Task<ActionResult<AccessStateResponse>> Get(Guid deviceId, CancellationToken ct)
    {
        var state = await _handler.GetStateAsync(deviceId, ct);
        return state is null ? NotFound(new { error = "No access state for this device" }) : Ok(AccessStateResponse.From(state));
    }

    // PUT /api/v1/access/{deviceId} - lock/unlock. Every call is audited.
    // Publishes AccessCommandRequested to RabbitMQ; the actual value is
    // updated later, asynchronously, when AccessCommandCompletedConsumer
    // receives the Device Gateway's ack.
    [HttpPut("{deviceId:guid}")]
    public async Task<ActionResult<AccessStateResponse>> SetDesired(Guid deviceId, [FromBody] SetAccessRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.DesiredValue))
        {
            return BadRequest(new { error = "desiredValue is required" });
        }

        var state = await _handler.SetDesiredAsync(deviceId, request, ct);
        return Ok(AccessStateResponse.From(state));
    }

    // GET /api/v1/access/{deviceId}/audit - who locked/unlocked this gate, and when.
    [HttpGet("{deviceId:guid}/audit")]
    public async Task<ActionResult<List<AccessAuditResponse>>> GetAudit(Guid deviceId, CancellationToken ct)
    {
        var entries = await _handler.GetAuditTrailAsync(deviceId, ct);
        return Ok(entries.Select(AccessAuditResponse.From));
    }
}
