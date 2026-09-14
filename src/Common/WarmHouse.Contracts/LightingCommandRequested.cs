namespace WarmHouse.Contracts;

/// <summary>
/// Published by the Lighting Control Service when a homeowner sets a
/// desired value; consumed by the Device Gateway.
/// </summary>
public record LightingCommandRequested(Guid DeviceId, Guid HouseId, string DesiredValue);
