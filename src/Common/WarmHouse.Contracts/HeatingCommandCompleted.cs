namespace WarmHouse.Contracts;

/// <summary>
/// Published by the Device Gateway once a heating command has been
/// delivered to the physical/partner device; consumed by the Heating
/// Control Service to record the actual (as opposed to desired) value.
/// </summary>
public record HeatingCommandCompleted(Guid DeviceId, string ActualValue);
