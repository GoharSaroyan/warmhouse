using Heating.Api.Domain;

namespace Heating.Api.Application;

public record HeatingStateResponse(Guid DeviceId, string DesiredValue, string ActualValue, DateTimeOffset UpdatedAt)
{
    public static HeatingStateResponse From(HeatingState s) => new(s.DeviceId, s.DesiredValue, s.ActualValue, s.UpdatedAt);
}

public record SetHeatingRequest(string DesiredValue);

/// <summary>
/// Owns turning heating on/off and tracking desired/actual state per
/// room. In the target architecture, SetDesiredAsync publishes a command
/// to the Message Broker for the Device Gateway to deliver (see
/// docs/c4/container-to-be.puml); ReportActualAsync is what a consumed
/// command-ack event would call. Both are direct/synchronous here until
/// the broker exists - see the TODO below.
/// </summary>
public class HeatingCommandHandler
{
    private readonly IHeatingStateRepository _repository;
    private readonly ILogger<HeatingCommandHandler> _logger;

    public HeatingCommandHandler(IHeatingStateRepository repository, ILogger<HeatingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public Task<HeatingState?> GetStateAsync(Guid deviceId, CancellationToken ct = default)
    {
        return _repository.GetAsync(deviceId, ct);
    }

    // TODO: once the Message Broker exists, publish a "heating command"
    // event here instead of writing the desired value directly, and let
    // ReportActualAsync be driven by the consumed command-ack event.
    public async Task<HeatingState> SetDesiredAsync(Guid deviceId, string desiredValue, CancellationToken ct = default)
    {
        var state = await _repository.SetDesiredAsync(deviceId, desiredValue, ct);
        _logger.LogInformation("Heating desired state for device {DeviceId} set to {Value}", deviceId, desiredValue);
        return state;
    }

    public Task<HeatingState?> ReportActualAsync(Guid deviceId, string actualValue, CancellationToken ct = default)
    {
        return _repository.SetActualAsync(deviceId, actualValue, ct);
    }
}
