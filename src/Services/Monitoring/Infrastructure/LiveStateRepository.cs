using Monitoring.Api.Domain;
using Npgsql;

namespace Monitoring.Api.Infrastructure;

public class LiveStateRepository : ILiveStateRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public LiveStateRepository(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    public async Task<LiveState?> GetAsync(Guid deviceId, CancellationToken ct = default)
    {
        const string sql = "SELECT device_id, house_id, value, status, updated_at FROM live_states WHERE device_id = @deviceId";

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("deviceId", deviceId);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Read(reader) : null;
    }

    public async Task<List<LiveState>> GetForHouseAsync(Guid houseId, CancellationToken ct = default)
    {
        const string sql = "SELECT device_id, house_id, value, status, updated_at FROM live_states WHERE house_id = @houseId ORDER BY updated_at DESC";

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("houseId", houseId);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var result = new List<LiveState>();
        while (await reader.ReadAsync(ct))
        {
            result.Add(Read(reader));
        }

        return result;
    }

    public async Task<LiveState> ReportAsync(Guid deviceId, Guid houseId, string value, string status, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO live_states (device_id, house_id, value, status, updated_at)
            VALUES (@deviceId, @houseId, @value, @status, @now)
            ON CONFLICT (device_id) DO UPDATE
            SET house_id = @houseId, value = @value, status = @status, updated_at = @now
            RETURNING device_id, house_id, value, status, updated_at
            """;

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("deviceId", deviceId);
        cmd.Parameters.AddWithValue("houseId", houseId);
        cmd.Parameters.AddWithValue("value", value);
        cmd.Parameters.AddWithValue("status", status);
        cmd.Parameters.AddWithValue("now", DateTimeOffset.UtcNow);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        await reader.ReadAsync(ct);
        return Read(reader);
    }

    private static LiveState Read(NpgsqlDataReader reader) => new()
    {
        DeviceId = reader.GetGuid(0),
        HouseId = reader.GetGuid(1),
        Value = reader.GetString(2),
        Status = reader.GetString(3),
        UpdatedAt = reader.GetFieldValue<DateTimeOffset>(4),
    };
}
