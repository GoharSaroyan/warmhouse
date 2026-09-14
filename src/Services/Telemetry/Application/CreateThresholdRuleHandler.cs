using Telemetry.Api.Domain;

namespace Telemetry.Api.Application;

public class CreateThresholdRuleHandler
{
    private readonly IThresholdRuleRepository _rules;

    public CreateThresholdRuleHandler(IThresholdRuleRepository rules) => _rules = rules;

    public Task<ThresholdRule> HandleAsync(CreateThresholdRuleRequest request, CancellationToken ct = default)
    {
        var rule = new ThresholdRule
        {
            HouseId = request.HouseId,
            DeviceId = request.DeviceId,
            Metric = request.Metric,
            Operator = request.Operator,
            Value = request.Value,
        };

        return _rules.CreateAsync(rule, ct);
    }
}
