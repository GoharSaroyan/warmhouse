namespace Heating.Api.Domain;

/// <summary>
/// Desired vs. actual heating state for one device (room). See
/// docs/erd/erd.puml's HeatingState entity.
/// </summary>
public class HeatingState
{
    public Guid DeviceId { get; set; }
    public string DesiredValue { get; set; } = "off";
    public string ActualValue { get; set; } = "off";
    public DateTimeOffset UpdatedAt { get; set; }
}
