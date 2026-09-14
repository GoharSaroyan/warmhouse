namespace WarmHouse.Contracts;

/// <summary>
/// Published by the Device Gateway once a gate command has been
/// delivered; consumed by the Access Control Service.
/// </summary>
public record AccessCommandCompleted(Guid DeviceId, string ActualValue);
