namespace Lighting.Api.Domain;

public interface ILightingStateRepository
{
    Task<LightingState?> GetAsync(Guid deviceId, CancellationToken ct = default);
    Task<LightingState> SetDesiredAsync(Guid deviceId, string desiredValue, CancellationToken ct = default);
    Task<LightingState?> SetActualAsync(Guid deviceId, string actualValue, CancellationToken ct = default);
}
