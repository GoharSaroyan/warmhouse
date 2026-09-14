using Monitoring.Api.Domain;

namespace Monitoring.Api.Application;

public record LiveStateResponse(Guid DeviceId, Guid HouseId, string Value, string Status, DateTimeOffset UpdatedAt)
{
    public static LiveStateResponse From(LiveState s) => new(s.DeviceId, s.HouseId, s.Value, s.Status, s.UpdatedAt);
}

public record ReportStateRequest(Guid HouseId, string Value, string Status);

/// <summary>
/// Owns letting a homeowner view the current, live state of their home
/// right now. ReportAsync is called by DeviceStateChangedConsumer, which
/// consumes the event the Device Gateway publishes over RabbitMQ (see
/// docs/c4/container-to-be.puml) - Monitoring never talks to devices or
/// the control services directly.
/// </summary>
public class MonitoringHandler
{
    private readonly ILiveStateRepository _repository;

    public MonitoringHandler(ILiveStateRepository repository) => _repository = repository;

    public Task<LiveState?> GetAsync(Guid deviceId, CancellationToken ct = default)
    {
        return _repository.GetAsync(deviceId, ct);
    }

    public Task<List<LiveState>> GetForHouseAsync(Guid houseId, CancellationToken ct = default)
    {
        return _repository.GetForHouseAsync(houseId, ct);
    }

    public Task<LiveState> ReportAsync(Guid deviceId, ReportStateRequest request, CancellationToken ct = default)
    {
        return _repository.ReportAsync(deviceId, request.HouseId, request.Value, request.Status, ct);
    }
}
