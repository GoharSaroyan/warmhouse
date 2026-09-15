using Heating.Api.Domain;
using MassTransit;
using WarmHouse.Contracts;

namespace Heating.Api.Application;

public record HeatingStateResponse(Guid DeviceId, string DesiredValue, string ActualValue, DateTimeOffset UpdatedAt)
{
    public static HeatingStateResponse From(HeatingState s) => new(s.DeviceId, s.DesiredValue, s.ActualValue, s.UpdatedAt);
}

public record SetHeatingRequest(Guid HouseId, string DesiredValue);

/// <summary>
/// Owns turning heating on/off and tracking desired/actual state per
/// room. SetDesiredAsync records the desired value immediately (so a
/// GET right after reflects the homeowner's intent) and publishes a
/// HeatingCommandRequested event for the Device Gateway to deliver over
/// RabbitMQ (see docs/c4/container-to-be.puml) - it does not wait for
/// the device to actually respond. ReportActualAsync is called by
/// HeatingCommandCompletedConsumer once that ack event comes back.
/// </summary>
public class HeatingCommandHandler
{
    private readonly IHeatingStateRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<HeatingCommandHandler> _logger;

    public HeatingCommandHandler(IHeatingStateRepository repository, IPublishEndpoint publishEndpoint, ILogger<HeatingCommandHandler> logger)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public Task<HeatingState?> GetStateAsync(Guid deviceId, CancellationToken ct = default)
    {
        return _repository.GetAsync(deviceId, ct);
    }

    public async Task<HeatingState> SetDesiredAsync(Guid deviceId, SetHeatingRequest request, CancellationToken ct = default)
    {
        var state = await _repository.SetDesiredAsync(deviceId, request.DesiredValue, ct);

        await _publishEndpoint.Publish(new HeatingCommandRequested(deviceId, request.HouseId, request.DesiredValue), ct);
        _logger.LogInformation("Published HeatingCommandRequested for device {DeviceId} -> {Value}", deviceId, request.DesiredValue);

        return state;
    }

    public Task<HeatingState?> ReportActualAsync(Guid deviceId, string actualValue, CancellationToken ct = default)
    {
        return _repository.SetActualAsync(deviceId, actualValue, ct);
    }
}
