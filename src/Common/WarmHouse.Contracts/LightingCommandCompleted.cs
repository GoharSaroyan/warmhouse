namespace WarmHouse.Contracts;

/// <summary>
/// Published by the Device Gateway once a lighting command has been
/// delivered; consumed by the Lighting Control Service.
/// </summary>
public record LightingCommandCompleted(Guid DeviceId, string ActualValue);
