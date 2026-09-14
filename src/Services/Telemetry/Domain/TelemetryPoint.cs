namespace Telemetry.Api.Domain;

/// <summary>
/// A single measurement. MeasuredAt and ReceivedAt are tracked
/// separately - a device may buffer readings before it reconnects. See
/// docs/c4/component-telemetry.puml.
/// </summary>
public class TelemetryPoint
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public string Metric { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTimeOffset MeasuredAt { get; set; }
    public DateTimeOffset ReceivedAt { get; set; }
}
