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

public record DeviceResponse(Guid Id, Guid ModuleId, Guid HouseId, string SerialNumber, string Status, DateTimeOffset InstalledAt)
{
    public static DeviceResponse From(Device d) => new(d.Id, d.ModuleId, d.HouseId, d.SerialNumber, d.Status, d.InstalledAt);
}

public record DeviceCreateRequest(Guid ModuleId, Guid HouseId, string SerialNumber);

public record DeviceUpdateRequest(string? SerialNumber, string? Status, Guid? HouseId);

/// <summary>
/// Request body for POST /onboard - the self-service pairing flow. The
/// homeowner has already bought a Module (a product); this pairs one
/// physical unit of it to their house.
/// </summary>
public record OnboardRequest(Guid HouseId, Guid ModuleId, string PairingCode);

public record OnboardResponse(bool Success, DeviceResponse? Device, string? Error);
