using DeviceManagement.Api.Application;
using DeviceManagement.Api.Domain;
using Microsoft.AspNetCore.Mvc;

namespace DeviceManagement.Api.Presentation;

/// <summary>
/// Device type catalog (heating, lighting, access-control, telemetry, ...).
/// Reached via the API Gateway at /api/v1/devices/types.
/// </summary>
[ApiController]
[Route("types")]
public class DeviceTypesController : ControllerBase
{
    private readonly IDeviceTypeRepository _repository;

    public DeviceTypesController(IDeviceTypeRepository repository) => _repository = repository;

    [HttpGet]
    public async Task<ActionResult<List<DeviceTypeResponse>>> GetAll(CancellationToken ct)
    {
        var types = await _repository.GetAllAsync(ct);
        return Ok(types.Select(DeviceTypeResponse.From));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DeviceTypeResponse>> GetById(Guid id, CancellationToken ct)
    {
        var type = await _repository.GetByIdAsync(id, ct);
        return type is null ? NotFound(new { error = "Device type not found" }) : Ok(DeviceTypeResponse.From(type));
    }

    [HttpPost]
    public async Task<ActionResult<DeviceTypeResponse>> Create([FromBody] DeviceTypeCreateRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { error = "name is required" });
        }

        var created = await _repository.CreateAsync(new DeviceType
        {
            Name = request.Name,
            Unit = request.Unit,
            Description = request.Description,
        }, ct);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, DeviceTypeResponse.From(created));
    }
}
