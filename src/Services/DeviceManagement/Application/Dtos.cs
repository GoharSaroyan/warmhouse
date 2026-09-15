using DeviceManagement.Api.Domain;

namespace DeviceManagement.Api.Application;

public record DeviceTypeResponse(Guid Id, string Name, string Unit, string Description)
{
    public static DeviceTypeResponse From(DeviceType t) => new(t.Id, t.Name, t.Unit, t.Description);
}

public record DeviceTypeCreateRequest(string Name, string Unit, string Description);

public record ModuleResponse(Guid Id, Guid DeviceTypeId, string Name, string Manufacturer, string Protocol, decimal Price)
{
    public static ModuleResponse From(Module m) => new(m.Id, m.DeviceTypeId, m.Name, m.Manufacturer, m.Protocol, m.Price);
}

public record ModuleCreateRequest(Guid DeviceTypeId, string Name, string Manufacturer, string Protocol, decimal Price);

/// <summary>
/// A device, plus - only when listed through GetAll's deviceType filter for
/// a readable sensor type ("Telemetry") - a freshly generated current
/// reading (Value/Unit/ReadingStatus/ReadingAt). Devices of other types, or
/// devices returned without that filter, simply leave those fields null.
/// </summary>
public record DeviceResponse(
    Guid Id, Guid ModuleId, Guid HouseId, string SerialNumber, string Status, DateTimeOffset InstalledAt,
    double? Value = null, string? Unit = null, string? ReadingStatus = null, DateTimeOffset? ReadingAt = null)
{
    public static DeviceResponse From(Device d) => new(d.Id, d.ModuleId, d.HouseId, d.SerialNumber, d.Status, d.InstalledAt);

    public static DeviceResponse From(Device d, SensorReading reading) => new(
        d.Id, d.ModuleId, d.HouseId, d.SerialNumber, d.Status, d.InstalledAt,
        reading.Value, reading.Unit, reading.Status, reading.ReadingAt);
}

/// <summary>
/// A simulated instantaneous reading for a "Telemetry"-type device (e.g. a
/// temperature sensor) - generated fresh on every GetAll call, the same way
/// a real remote sensor would return a new value on every poll. See Task 5.
/// </summary>
public record SensorReading(double Value, string Unit, string Status, DateTimeOffset ReadingAt);

public record DeviceCreateRequest(Guid ModuleId, Guid HouseId, string SerialNumber);

public record DeviceUpdateRequest(string? SerialNumber, string? Status, Guid? HouseId);

/// <summary>
/// Request body for POST /onboard - the self-service pairing flow. The
/// homeowner has already bought a Module (a product); this pairs one
/// physical unit of it to their house.
/// </summary>
public record OnboardRequest(Guid HouseId, Guid ModuleId, string PairingCode);

public record OnboardResponse(bool Success, DeviceResponse? Device, string? Error);
