using Lighting.Api.Domain;

namespace Lighting.Api.Application;

public record LightingStateResponse(Guid DeviceId, string DesiredValue, string ActualValue, DateTimeOffset UpdatedAt)
{
    public static LightingStateResponse From(LightingState s) => new(s.DeviceId, s.DesiredValue, s.ActualValue, s.UpdatedAt);
}

public record SetLightingRequest(string DesiredValue);

/// <summary>
/// Owns turning lights on/off per room. See the TODO on Heating's
/// equivalent handler - the same Message Broker migration applies here.
/// </summary>
public class LightingCommandHandler
{
    private readonly ILightingStateRepository _repository;
    private readonly ILogger<LightingCommandHandler> _logger;

    public LightingCommandHandler(ILightingStateRepository repository, ILogger<LightingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public Task<LightingState?> GetStateAsync(Guid deviceId, CancellationToken ct = default)
    {
        return _repository.GetAsync(deviceId, ct);
    }

    public async Task<LightingState> SetDesiredAsync(Guid deviceId, string desiredValue, CancellationToken ct = default)
    {
        var state = await _repository.SetDesiredAsync(deviceId, desiredValue, ct);
        _logger.LogInformation("Lighting desired state for device {DeviceId} set to {Value}", deviceId, desiredValue);
        return state;
    }

    public Task<LightingState?> ReportActualAsync(Guid deviceId, string actualValue, CancellationToken ct = default)
    {
        return _repository.SetActualAsync(deviceId, actualValue, ct);
    }
}
