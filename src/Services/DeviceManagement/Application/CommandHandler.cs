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
    private readonly IDeviceTypeRepository _deviceTypes;
    private readonly Random _random = Random.Shared;

    /// <summary>Device type that reports readings (e.g. temperature sensors) - see Task 5.</summary>
    private const string ReadableDeviceTypeName = "Telemetry";

    public CommandHandler(DeviceStateManager stateManager, IDeviceTypeRepository deviceTypes)
    {
        _stateManager = stateManager;
        _deviceTypes = deviceTypes;
    }

    /// <summary>
    /// Lists devices, optionally scoped to one house and/or one device type
    /// (by name, e.g. "Telemetry", "Heating"). When the resolved type is the
    /// readable "Telemetry" type, each device also gets a freshly generated
    /// current reading (Task 5: "Get All Sensors" - a different value on
    /// every call, in place of a separate temperature-api).
    /// </summary>
    public async Task<List<DeviceResponse>> GetDevicesAsync(Guid? houseId, string? deviceType, CancellationToken ct = default)
    {
        Guid? deviceTypeId = null;
        var isReadableType = false;

        if (!string.IsNullOrWhiteSpace(deviceType))
        {
            var types = await _deviceTypes.GetAllAsync(ct);
            var match = types.FirstOrDefault(t => t.Name.Equals(deviceType, StringComparison.OrdinalIgnoreCase));
            if (match is null)
            {
                return [];
            }

            deviceTypeId = match.Id;
            isReadableType = match.Name.Equals(ReadableDeviceTypeName, StringComparison.OrdinalIgnoreCase);
        }

        var devices = await _stateManager.GetDevicesAsync(houseId, deviceTypeId, ct);
        if (!isReadableType)
        {
            return devices.Select(DeviceResponse.From).ToList();
        }

        return devices.Select(d => DeviceResponse.From(d, GenerateReading())).ToList();
    }

    // Simulates a remote sensor: a random value in a plausible range,
    // different on every call - same contract Task 5 originally asked of a
    // standalone temperature-api, just generated in-process instead.
    private SensorReading GenerateReading()
    {
        var value = Math.Round(_random.NextDouble() * 15 + 15, 1); // 15.0-30.0
        var status = value is >= 18 and <= 26 ? "normal" : "warning";
        return new SensorReading(value, "°C", status, DateTimeOffset.UtcNow);
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
