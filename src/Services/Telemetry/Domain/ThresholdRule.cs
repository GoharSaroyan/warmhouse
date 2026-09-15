namespace Telemetry.Api.Domain;

/// <summary>
/// gt / gte / lt / lte operator, scoped to one device or the whole home
/// (DeviceId null). See docs/c4/component-telemetry.puml.
/// </summary>
public class ThresholdRule
{
    public Guid Id { get; set; }
    public Guid HouseId { get; set; }
    public Guid? DeviceId { get; set; }
    public string Metric { get; set; } = string.Empty;
    public string Operator { get; set; } = "gt";
    public double Value { get; set; }

    /// <summary>The domain rule: does this point breach the threshold?</summary>
    public bool IsBreachedBy(TelemetryPoint point)
    {
        if (!string.Equals(point.Metric, Metric, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (DeviceId is not null && DeviceId != point.DeviceId)
        {
            return false;
        }

        return Operator switch
        {
            "gt" => point.Value > Value,
            "gte" => point.Value >= Value,
            "lt" => point.Value < Value,
            "lte" => point.Value <= Value,
            _ => false,
        };
    }
}
