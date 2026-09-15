namespace WarmHouse.Contracts;

/// <summary>
/// Published by the Heating Control Service when a homeowner sets a
/// desired value; consumed by the Device Gateway. See
/// docs/c4/container-to-be.puml.
/// </summary>
public record HeatingCommandRequested(Guid DeviceId, Guid HouseId, string DesiredValue);
