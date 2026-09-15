namespace Monitoring.Api.Domain;

/// <summary>
/// The latest known state of one device, for homeowner viewing "right
/// now" - as opposed to Telemetry's historical aggregation. See Task 1's
/// domain write-up and docs/erd/erd.puml.
/// </summary>
public class LiveState
{
    public Guid DeviceId { get; set; }
    public Guid HouseId { get; set; }
    public string Value { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
}
