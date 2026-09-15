using Identity.Api.Application;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Presentation;

/// <summary>Reached via the API Gateway at /api/v1/identity/houses.</summary>
[ApiController]
[Route("houses")]
public class HousesController : ControllerBase
{
    private readonly HouseHandler _handler;

    public HousesController(HouseHandler handler) => _handler = handler;

    [HttpGet]
    public async Task<ActionResult<List<HouseResponse>>> GetForUser([FromQuery] Guid userId, CancellationToken ct)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest(new { error = "userId is required" });
        }

        var houses = await _handler.GetForUserAsync(userId, ct);
        return Ok(houses.Select(HouseResponse.From));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<HouseResponse>> GetById(Guid id, CancellationToken ct)
    {
        var house = await _handler.GetByIdAsync(id, ct);
        return house is null ? NotFound(new { error = "House not found" }) : Ok(HouseResponse.From(house));
    }

    [HttpPost]
    public async Task<ActionResult<HouseResponse>> Create([FromBody] CreateHouseRequest request, CancellationToken ct)
    {
        try
        {
            var house = await _handler.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = house.Id }, HouseResponse.From(house));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
