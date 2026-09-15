namespace DeviceManagement.Api.Domain;

/// <summary>
/// A category of device (heating, lighting, access-control, telemetry, ...).
/// See docs/erd/erd.puml.
/// </summary>
public class DeviceType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
