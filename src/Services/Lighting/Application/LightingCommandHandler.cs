using Lighting.Api.Domain;
using MassTransit;
using WarmHouse.Contracts;

namespace Lighting.Api.Application;

public record LightingStateResponse(Guid DeviceId, string DesiredValue, string ActualValue, DateTimeOffset UpdatedAt)
{
    public static LightingStateResponse From(LightingState s) => new(s.DeviceId, s.DesiredValue, s.ActualValue, s.UpdatedAt);
}

public record SetLightingRequest(Guid HouseId, string DesiredValue);

/// <summary>
/// Owns turning lights on/off per room. SetDesiredAsync records the
/// desired value immediately and publishes LightingCommandRequested for
/// the Device Gateway to deliver over RabbitMQ; ReportActualAsync is
/// called by LightingCommandCompletedConsumer once the ack comes back.
/// </summary>
public class LightingCommandHandler
{
    private readonly ILightingStateRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<LightingCommandHandler> _logger;

    public LightingCommandHandler(ILightingStateRepository repository, IPublishEndpoint publishEndpoint, ILogger<LightingCommandHandler> logger)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public Task<LightingState?> GetStateAsync(Guid deviceId, CancellationToken ct = default)
    {
        return _repository.GetAsync(deviceId, ct);
    }

    public async Task<LightingState> SetDesiredAsync(Guid deviceId, SetLightingRequest request, CancellationToken ct = default)
    {
        var state = await _repository.SetDesiredAsync(deviceId, request.DesiredValue, ct);

        await _publishEndpoint.Publish(new LightingCommandRequested(deviceId, request.HouseId, request.DesiredValue), ct);
        _logger.LogInformation("Published LightingCommandRequested for device {DeviceId} -> {Value}", deviceId, request.DesiredValue);

        return state;
    }

    public Task<LightingState?> ReportActualAsync(Guid deviceId, string actualValue, CancellationToken ct = default)
    {
        return _repository.SetActualAsync(deviceId, actualValue, ct);
    }
}
