namespace DeviceManagement.Api.Domain;

public interface IDeviceTypeRepository
{
    Task<List<DeviceType>> GetAllAsync(CancellationToken ct = default);
    Task<DeviceType?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<DeviceType> CreateAsync(DeviceType deviceType, CancellationToken ct = default);
}
