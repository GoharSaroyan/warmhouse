namespace Monitoring.Api.Domain;

public interface ILiveStateRepository
{
    Task<LiveState?> GetAsync(Guid deviceId, CancellationToken ct = default);
    Task<List<LiveState>> GetForHouseAsync(Guid houseId, CancellationToken ct = default);
    Task<LiveState> ReportAsync(Guid deviceId, Guid houseId, string value, string status, CancellationToken ct = default);
}
