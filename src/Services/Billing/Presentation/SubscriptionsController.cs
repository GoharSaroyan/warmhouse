using Billing.Api.Application;
using Microsoft.AspNetCore.Mvc;

namespace Billing.Api.Presentation;

/// <summary>Reached via the API Gateway at /api/v1/billing.</summary>
[ApiController]
[Route("")]
public class SubscriptionsController : ControllerBase
{
    private readonly SubscriptionHandler _handler;

    public SubscriptionsController(SubscriptionHandler handler) => _handler = handler;

    [HttpGet]
    public async Task<ActionResult<List<SubscriptionResponse>>> GetForUser([FromQuery] Guid userId, CancellationToken ct)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest(new { error = "userId is required" });
        }

        var subscriptions = await _handler.GetForUserAsync(userId, ct);
        return Ok(subscriptions.Select(SubscriptionResponse.From));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SubscriptionResponse>> GetById(Guid id, CancellationToken ct)
    {
        var subscription = await _handler.GetByIdAsync(id, ct);
        return subscription is null ? NotFound(new { error = "Subscription not found" }) : Ok(SubscriptionResponse.From(subscription));
    }

    [HttpPost]
    public async Task<ActionResult<SubscriptionResponse>> Subscribe([FromBody] CreateSubscriptionRequest request, CancellationToken ct)
    {
        try
        {
            var subscription = await _handler.SubscribeAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = subscription.Id }, SubscriptionResponse.From(subscription));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // PUT /api/v1/billing/{id} - only "cancelled" is meaningful today.
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SubscriptionResponse>> Update(Guid id, [FromBody] UpdateSubscriptionRequest request, CancellationToken ct)
    {
        if (!string.Equals(request.Status, "cancelled", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Only 'cancelled' is supported" });
        }

        var subscription = await _handler.CancelAsync(id, ct);
        return subscription is null ? NotFound(new { error = "Subscription not found" }) : Ok(SubscriptionResponse.From(subscription));
    }
}
