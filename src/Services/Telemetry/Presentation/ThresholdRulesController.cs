using Microsoft.AspNetCore.Mvc;
using Telemetry.Api.Application;
using Telemetry.Api.Domain;

namespace Telemetry.Api.Presentation;

/// <summary>Reached via the API Gateway at /api/v1/telemetry/rules.</summary>
[ApiController]
[Route("rules")]
public class ThresholdRulesController : ControllerBase
{
    private readonly IThresholdRuleRepository _rules;
    private readonly CreateThresholdRuleHandler _create;

    public ThresholdRulesController(IThresholdRuleRepository rules, CreateThresholdRuleHandler create)
    {
        _rules = rules;
        _create = create;
    }

    [HttpGet]
    public async Task<ActionResult<List<ThresholdRuleResponse>>> GetAll([FromQuery] Guid houseId, CancellationToken ct)
    {
        var rules = await _rules.GetForHouseAsync(houseId, ct);
        return Ok(rules.Select(ThresholdRuleResponse.From));
    }

    [HttpPost]
    public async Task<ActionResult<ThresholdRuleResponse>> Create([FromBody] CreateThresholdRuleRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Metric))
        {
            return BadRequest(new { error = "metric is required" });
        }

        var rule = await _create.HandleAsync(request, ct);
        return Ok(ThresholdRuleResponse.From(rule));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _rules.DeleteAsync(id, ct);
        return deleted ? Ok(new { message = "Rule deleted successfully" }) : NotFound(new { error = "Rule not found" });
    }
}
