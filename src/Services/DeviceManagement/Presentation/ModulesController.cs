using DeviceManagement.Api.Application;
using DeviceManagement.Api.Domain;
using Microsoft.AspNetCore.Mvc;

namespace DeviceManagement.Api.Presentation;

/// <summary>
/// Purchasable device products (e.g. "SmartTherm X1"), each linked to a
/// DeviceType. Reached via the API Gateway at /api/v1/devices/modules.
/// </summary>
[ApiController]
[Route("modules")]
public class ModulesController : ControllerBase
{
    private readonly IModuleRepository _repository;

    public ModulesController(IModuleRepository repository) => _repository = repository;

    [HttpGet]
    public async Task<ActionResult<List<ModuleResponse>>> GetAll([FromQuery] Guid? deviceTypeId, CancellationToken ct)
    {
        var modules = await _repository.GetAllAsync(deviceTypeId, ct);
        return Ok(modules.Select(ModuleResponse.From));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ModuleResponse>> GetById(Guid id, CancellationToken ct)
    {
        var module = await _repository.GetByIdAsync(id, ct);
        return module is null ? NotFound(new { error = "Module not found" }) : Ok(ModuleResponse.From(module));
    }

    [HttpPost]
    public async Task<ActionResult<ModuleResponse>> Create([FromBody] ModuleCreateRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { error = "name is required" });
        }

        var created = await _repository.CreateAsync(new Module
        {
            DeviceTypeId = request.DeviceTypeId,
            Name = request.Name,
            Manufacturer = request.Manufacturer,
            Protocol = request.Protocol,
            Price = request.Price,
        }, ct);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ModuleResponse.From(created));
    }
}
