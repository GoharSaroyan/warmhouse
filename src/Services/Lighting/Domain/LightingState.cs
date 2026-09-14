namespace Lighting.Api.Domain;

/// <summary>
/// Desired vs. actual lighting state for one device (room). See
/// docs/erd/erd.puml's DeviceState entity.
/// </summary>
public class LightingState
{
    public Guid DeviceId { get; set; }
    public string DesiredValue { get; set; } = "off";
    public string ActualValue { get; set; } = "off";
    public DateTimeOffset UpdatedAt { get; set; }
}
