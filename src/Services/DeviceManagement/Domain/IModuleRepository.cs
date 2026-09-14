namespace DeviceManagement.Api.Domain;

public interface IModuleRepository
{
    Task<List<Module>> GetAllAsync(Guid? deviceTypeId, CancellationToken ct = default);
    Task<Module?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Module> CreateAsync(Module module, CancellationToken ct = default);
}
