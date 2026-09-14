using DeviceManagement.Api.Application;
using Microsoft.AspNetCore.Mvc;

namespace DeviceManagement.Api.Presentation;

/// <summary>
/// Device catalog and self-service onboarding. Reached via the API
/// Gateway at /api/v1/devices - see docs/c4/component-device-management.puml
/// and docs/c4/code-device-onboarding.puml.
/// </summary>
[ApiController]
[Route("")]
public class DevicesController : ControllerBase
{
    private readonly CommandHandler _commandHandler;
    private readonly OnboardingHandler _onboardingHandler;

    public DevicesController(CommandHandler commandHandler, OnboardingHandler onboardingHandler)
    {
        _commandHandler = commandHandler;
        _onboardingHandler = onboardingHandler;
    }

    // GET /api/v1/devices?houseId=...
    [HttpGet]
    public async Task<ActionResult<List<DeviceResponse>>> GetAll([FromQuery] Guid? houseId, CancellationToken ct)
    {
        var devices = await _commandHandler.GetDevicesAsync(houseId, ct);
        return Ok(devices.Select(DeviceResponse.From));
    }

    // GET /api/v1/devices/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DeviceResponse>> GetById(Guid id, CancellationToken ct)
    {
        var device = await _commandHandler.GetDeviceAsync(id, ct);
        return device is null ? NotFound(new { error = "Device not found" }) : Ok(DeviceResponse.From(device));
    }

    // POST /api/v1/devices - direct registration (e.g. admin/back-office use), as
    // opposed to /onboard which runs the homeowner-facing pairing flow.
    [HttpPost]
    public async Task<ActionResult<DeviceResponse>> Create([FromBody] DeviceCreateRequest request, CancellationToken ct)
    {
        try
        {
            var device = await _commandHandler.CreateDeviceAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = device.Id }, DeviceResponse.From(device));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // PUT /api/v1/devices/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DeviceResponse>> Update(Guid id, [FromBody] DeviceUpdateRequest request, CancellationToken ct)
    {
        var device = await _commandHandler.UpdateDeviceAsync(id, request, ct);
        return device is null ? NotFound(new { error = "Device not found" }) : Ok(DeviceResponse.From(device));
    }

    // DELETE /api/v1/devices/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _commandHandler.DeleteDeviceAsync(id, ct);
        return deleted ? Ok(new { message = "Device deleted successfully" }) : NotFound(new { error = "Device not found" });
    }

    // POST /api/v1/devices/onboard - self-service pairing flow. See
    // docs/c4/code-device-onboarding.puml for the full sequence.
    [HttpPost("onboard")]
    public async Task<ActionResult<OnboardResponse>> Onboard([FromBody] OnboardRequest request, CancellationToken ct)
    {
        var result = await _onboardingHandler.StartOnboardingAsync(request, ct);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Device!.Id }, result) : BadRequest(result);
    }
}
