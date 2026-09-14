using DeviceManagement.Api.Application;
using DeviceManagement.Api.Presentation.Examples;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace DeviceManagement.Api.Presentation;

/// <summary>
/// Device catalog and self-service onboarding. Reached via the API
/// Gateway at /api/v1/devices - see docs/c4/component-device-management.puml
/// and docs/c4/code-device-onboarding.puml.
/// </summary>
[ApiController]
[Route("")]
[Produces("application/json")]
public class DevicesController : ControllerBase
{
    private readonly CommandHandler _commandHandler;
    private readonly OnboardingHandler _onboardingHandler;

    public DevicesController(CommandHandler commandHandler, OnboardingHandler onboardingHandler)
    {
        _commandHandler = commandHandler;
        _onboardingHandler = onboardingHandler;
    }

    /// <summary>Lists devices, optionally scoped to one house.</summary>
    /// <param name="houseId">When given, only devices belonging to this house are returned.</param>
    /// <response code="200">The (possibly empty) list of matching devices.</response>
    // GET /api/v1/devices?houseId=...
    [HttpGet]
    [ProducesResponseType(typeof(List<DeviceResponse>), StatusCodes.Status200OK)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(DeviceListResponseExample))]
    public async Task<ActionResult<List<DeviceResponse>>> GetAll([FromQuery] Guid? houseId, CancellationToken ct)
    {
        var devices = await _commandHandler.GetDevicesAsync(houseId, ct);
        return Ok(devices.Select(DeviceResponse.From));
    }

    /// <summary>Gets one device by id.</summary>
    /// <param name="id">The device's id.</param>
    /// <response code="200">The device.</response>
    /// <response code="404">No device exists with this id.</response>
    // GET /api/v1/devices/{id}
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DeviceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(DeviceResponseExample))]
    public async Task<ActionResult<DeviceResponse>> GetById(Guid id, CancellationToken ct)
    {
        var device = await _commandHandler.GetDeviceAsync(id, ct);
        return device is null ? NotFound(new { error = "Device not found" }) : Ok(DeviceResponse.From(device));
    }

    /// <summary>
    /// Registers a device directly (e.g. admin/back-office use), as opposed
    /// to <see cref="Onboard"/> which runs the homeowner-facing self-service
    /// pairing flow.
    /// </summary>
    /// <param name="request">The module being installed, which house it goes in, and its serial number.</param>
    /// <response code="201">The device was created; the Location header points at GET /{id}.</response>
    /// <response code="400">serial_number was missing, or module_id does not reference an existing module.</response>
    // POST /api/v1/devices
    [HttpPost]
    [ProducesResponseType(typeof(DeviceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerRequestExample(typeof(DeviceCreateRequest), typeof(DeviceCreateRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status201Created, typeof(DeviceResponseExample))]
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

    /// <summary>Updates a device's serial number, status and/or owning house. Fields left null are unchanged.</summary>
    /// <param name="id">The device's id.</param>
    /// <param name="request">Only the fields being changed need to be set.</param>
    /// <response code="200">The device as it now stands.</response>
    /// <response code="404">No device exists with this id.</response>
    // PUT /api/v1/devices/{id}
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(DeviceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerRequestExample(typeof(DeviceUpdateRequest), typeof(DeviceUpdateRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(DeviceResponseExample))]
    public async Task<ActionResult<DeviceResponse>> Update(Guid id, [FromBody] DeviceUpdateRequest request, CancellationToken ct)
    {
        var device = await _commandHandler.UpdateDeviceAsync(id, request, ct);
        return device is null ? NotFound(new { error = "Device not found" }) : Ok(DeviceResponse.From(device));
    }

    /// <summary>Removes a device from the catalog.</summary>
    /// <param name="id">The device's id.</param>
    /// <response code="200">The device was deleted.</response>
    /// <response code="404">No device exists with this id.</response>
    // DELETE /api/v1/devices/{id}
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _commandHandler.DeleteDeviceAsync(id, ct);
        return deleted ? Ok(new { message = "Device deleted successfully" }) : NotFound(new { error = "Device not found" });
    }

    /// <summary>
    /// Runs the self-service pairing flow: a homeowner who already bought a
    /// Module pairs one physical unit of it to their house using a pairing
    /// code, with no technician required. See docs/c4/code-device-onboarding.puml
    /// for the full sequence, including the failure path.
    /// </summary>
    /// <param name="request">Which house, which module, and the pairing code shown on/with the device.</param>
    /// <response code="201">Pairing succeeded; the new device is returned.</response>
    /// <response code="400">Pairing failed - e.g. the pairing code was rejected or the device did not respond. The body's "success" field is false and "error" explains why.</response>
    // POST /api/v1/devices/onboard
    [HttpPost("onboard")]
    [ProducesResponseType(typeof(OnboardResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(OnboardResponse), StatusCodes.Status400BadRequest)]
    [SwaggerRequestExample(typeof(OnboardRequest), typeof(OnboardRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status201Created, typeof(OnboardResponseSuccessExample))]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(OnboardResponseFailureExample))]
    public async Task<ActionResult<OnboardResponse>> Onboard([FromBody] OnboardRequest request, CancellationToken ct)
    {
        var result = await _onboardingHandler.StartOnboardingAsync(request, ct);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Device!.Id }, result) : BadRequest(result);
    }
}
