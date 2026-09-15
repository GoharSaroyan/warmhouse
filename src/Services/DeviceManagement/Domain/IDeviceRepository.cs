namespace DeviceManagement.Api.Domain;

public interface IDeviceRepository
{
    Task<List<Device>> GetAllAsync(Guid? houseId, Guid? deviceTypeId, CancellationToken ct = default);
    Task<Device?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Device> CreateAsync(Device device, CancellationToken ct = default);
    Task<Device?> UpdateAsync(Guid id, Action<Device> apply, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
