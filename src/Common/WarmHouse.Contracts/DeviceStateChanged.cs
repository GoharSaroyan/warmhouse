namespace WarmHouse.Contracts;

/// <summary>
/// Published by the Device Gateway whenever a device's actual state
/// changes, regardless of device type; consumed by the Monitoring
/// Service so a homeowner can see the current state of their home.
/// </summary>
public record DeviceStateChanged(Guid DeviceId, Guid HouseId, string Value, string Status);
