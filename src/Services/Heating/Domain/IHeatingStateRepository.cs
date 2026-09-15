namespace Heating.Api.Domain;

public interface IHeatingStateRepository
{
    Task<HeatingState?> GetAsync(Guid deviceId, CancellationToken ct = default);
    Task<HeatingState> SetDesiredAsync(Guid deviceId, string desiredValue, CancellationToken ct = default);
    Task<HeatingState?> SetActualAsync(Guid deviceId, string actualValue, CancellationToken ct = default);
}
