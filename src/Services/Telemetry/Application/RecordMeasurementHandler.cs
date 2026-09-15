using Telemetry.Api.Domain;

namespace Telemetry.Api.Application;

/// <summary>
/// Persists a measurement and evaluates the threshold rules that apply
/// to it. See docs/c4/component-telemetry.puml.
/// </summary>
public class RecordMeasurementHandler
{
    private readonly ITelemetryPointRepository _points;
    private readonly IThresholdRuleRepository _rules;
    private readonly ILogger<RecordMeasurementHandler> _logger;

    public RecordMeasurementHandler(ITelemetryPointRepository points, IThresholdRuleRepository rules, ILogger<RecordMeasurementHandler> logger)
    {
        _points = points;
        _rules = rules;
        _logger = logger;
    }

    public async Task<TelemetryPoint> HandleAsync(IngestRequest request, CancellationToken ct = default)
    {
        var point = new TelemetryPoint
        {
            Id = Guid.NewGuid(),
            DeviceId = request.DeviceId,
            Metric = request.Metric,
            Value = request.Value,
            Unit = request.Unit,
            MeasuredAt = request.MeasuredAt ?? DateTimeOffset.UtcNow,
            ReceivedAt = DateTimeOffset.UtcNow,
        };

        await _points.AddAsync(point, ct);

        var applicable = await _rules.GetApplicableRulesAsync(request.HouseId, request.DeviceId, request.Metric, ct);
        foreach (var rule in applicable.Where(r => r.IsBreachedBy(point)))
        {
            // TODO: publish TelemetryThresholdBreached to the Message
            // Broker once it exists, instead of just logging.
            _logger.LogWarning(
                "Threshold breached: device {DeviceId} metric {Metric} = {Value} {Operator} {Threshold} (rule {RuleId})",
                point.DeviceId, point.Metric, point.Value, rule.Operator, rule.Value, rule.Id);
        }

        return point;
    }
}
