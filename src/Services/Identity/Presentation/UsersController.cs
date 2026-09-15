using Identity.Api.Application;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Presentation;

/// <summary>Reached via the API Gateway at /api/v1/identity/users.</summary>
[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly UserHandler _handler;

    public UsersController(UserHandler handler) => _handler = handler;

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetById(Guid id, CancellationToken ct)
    {
        var user = await _handler.GetByIdAsync(id, ct);
        return user is null ? NotFound(new { error = "User not found" }) : Ok(UserResponse.From(user));
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Register([FromBody] RegisterUserRequest request, CancellationToken ct)
    {
        try
        {
            var user = await _handler.RegisterAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, UserResponse.From(user));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
