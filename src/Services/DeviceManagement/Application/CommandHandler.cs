using DeviceManagement.Api.Domain;

namespace DeviceManagement.Api.Application;

/// <summary>
/// The "Command Handler" component from
/// docs/c4/component-device-management.puml. Executes the direct
/// create/update/delete device use cases (as opposed to self-service
/// pairing, which is OnboardingHandler's job).
/// </summary>
public class CommandHandler
{
    private readonly DeviceStateManager _stateManager;

    public CommandHandler(DeviceStateManager stateManager)
    {
        _stateManager = stateManager;
    }

    public Task<List<Device>> GetDevicesAsync(Guid? houseId, CancellationToken ct = default)
    {
        return _stateManager.GetDevicesAsync(houseId, ct);
    }

    public Task<Device?> GetDeviceAsync(Guid id, CancellationToken ct = default)
    {
        return _stateManager.GetDeviceAsync(id, ct);
    }

    public async Task<Device> CreateDeviceAsync(DeviceCreateRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.SerialNumber))
        {
            throw new ArgumentException("serial_number is required");
        }

        return await _stateManager.RegisterDeviceAsync(request.ModuleId, request.HouseId, request.SerialNumber, "active", ct);
    }

    public Task<Device?> UpdateDeviceAsync(Guid id, DeviceUpdateRequest request, CancellationToken ct = default)
    {
        return _stateManager.UpdateDeviceAsync(id, request.SerialNumber, request.Status, request.HouseId, ct);
    }

    public Task<bool> DeleteDeviceAsync(Guid id, CancellationToken ct = default)
    {
        return _stateManager.DeleteDeviceAsync(id, ct);
    }
}
