namespace AccessControl.Api.Domain;

public interface IAccessStateRepository
{
    Task<AccessState?> GetAsync(Guid deviceId, CancellationToken ct = default);
    Task<AccessState> SetDesiredAsync(Guid deviceId, string desiredValue, CancellationToken ct = default);
    Task<AccessState?> SetActualAsync(Guid deviceId, string actualValue, CancellationToken ct = default);
}
