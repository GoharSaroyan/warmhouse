using Telemetry.Api.Domain;

namespace Telemetry.Api.Application;

public record TelemetryPointResponse(Guid Id, Guid DeviceId, string Metric, double Value, string Unit, DateTimeOffset MeasuredAt, DateTimeOffset ReceivedAt)
{
    public static TelemetryPointResponse From(TelemetryPoint p) => new(p.Id, p.DeviceId, p.Metric, p.Value, p.Unit, p.MeasuredAt, p.ReceivedAt);
}

// HouseId is carried on the request rather than looked up cross-service -
// in the target design the Device Gateway already knows it and includes
// it on the TelemetryReported event it publishes.
public record IngestRequest(Guid HouseId, Guid DeviceId, string Metric, double Value, string Unit, DateTimeOffset? MeasuredAt);

public record ThresholdRuleResponse(Guid Id, Guid HouseId, Guid? DeviceId, string Metric, string Operator, double Value)
{
    public static ThresholdRuleResponse From(ThresholdRule r) => new(r.Id, r.HouseId, r.DeviceId, r.Metric, r.Operator, r.Value);
}

public record CreateThresholdRuleRequest(Guid HouseId, Guid? DeviceId, string Metric, string Operator, double Value);
