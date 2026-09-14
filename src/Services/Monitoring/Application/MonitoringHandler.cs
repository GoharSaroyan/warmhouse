using Monitoring.Api.Domain;

namespace Monitoring.Api.Application;

public record LiveStateResponse(Guid DeviceId, Guid HouseId, string Value, string Status, DateTimeOffset UpdatedAt)
{
    public static LiveStateResponse From(LiveState s) => new(s.DeviceId, s.HouseId, s.Value, s.Status, s.UpdatedAt);
}

public record ReportStateRequest(Guid HouseId, string Value, string Status);

/// <summary>
/// Owns letting a homeowner view the current, live state of their home
/// right now. In the target architecture, ReportAsync is driven by
/// consuming live state-change events off the Message Broker (see
/// docs/c4/container-to-be.puml); it's a direct call here until that
/// broker exists.
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

    // TODO: replace with a Message Broker consumer once it exists.
    public Task<LiveState> ReportAsync(Guid deviceId, ReportStateRequest request, CancellationToken ct = default)
    {
        return _repository.ReportAsync(deviceId, request.HouseId, request.Value, request.Status, ct);
    }
}
