namespace DeviceManagement.Api.Domain;

/// <summary>
/// The "Device State Manager" component from
/// docs/c4/component-device-management.puml. Owns the in-process device
/// model: reads and persists device records, and enforces the invariants
/// that don't belong in a single repository call (e.g. "the module must
/// exist before a device can reference it").
/// </summary>
public class DeviceStateManager
{
    private readonly IDeviceRepository _devices;
    private readonly IModuleRepository _modules;

    public DeviceStateManager(IDeviceRepository devices, IModuleRepository modules)
    {
        _devices = devices;
        _modules = modules;
    }

    public Task<List<Device>> GetDevicesAsync(Guid? houseId, Guid? deviceTypeId, CancellationToken ct = default)
    {
        return _devices.GetAllAsync(houseId, deviceTypeId, ct);
    }

    public Task<Device?> GetDeviceAsync(Guid id, CancellationToken ct = default)
    {
        return _devices.GetByIdAsync(id, ct);
    }

    /// <summary>
    /// Registers a new device record. Used both by the direct "create
    /// device" command and by the self-service onboarding flow once a
    /// device has been paired.
    /// </summary>
    public async Task<Device> RegisterDeviceAsync(Guid moduleId, Guid houseId, string serialNumber, string status, CancellationToken ct = default)
    {
        var module = await _modules.GetByIdAsync(moduleId, ct)
            ?? throw new InvalidOperationException($"Module '{moduleId}' does not exist");

        var device = new Device
        {
            Id = Guid.NewGuid(),
            ModuleId = module.Id,
            HouseId = houseId,
            SerialNumber = serialNumber,
            Status = status,
            InstalledAt = DateTimeOffset.UtcNow,
        };

        return await _devices.CreateAsync(device, ct);
    }

    public Task<Device?> UpdateDeviceAsync(Guid id, string? serialNumber, string? status, Guid? houseId, CancellationToken ct = default)
    {
        return _devices.UpdateAsync(id, device =>
        {
            if (serialNumber is not null) device.SerialNumber = serialNumber;
            if (status is not null) device.Status = status;
            if (houseId is not null) device.HouseId = houseId.Value;
        }, ct);
    }

    public Task<bool> DeleteDeviceAsync(Guid id, CancellationToken ct = default)
    {
        return _devices.DeleteAsync(id, ct);
    }
}
