namespace WarmHouse.Contracts;

/// <summary>
/// Published by the Access Control Service when a homeowner locks/unlocks
/// a gate; consumed by the Device Gateway.
/// </summary>
public record AccessCommandRequested(Guid DeviceId, Guid HouseId, string DesiredValue, string Actor);
