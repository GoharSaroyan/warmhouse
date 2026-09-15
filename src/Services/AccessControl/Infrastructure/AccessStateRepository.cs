using AccessControl.Api.Domain;
using Npgsql;

namespace AccessControl.Api.Infrastructure;

public class AccessStateRepository : IAccessStateRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public AccessStateRepository(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    public async Task<AccessState?> GetAsync(Guid deviceId, CancellationToken ct = default)
    {
        const string sql = "SELECT device_id, desired_value, actual_value, updated_at FROM access_states WHERE device_id = @deviceId";

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("deviceId", deviceId);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Read(reader) : null;
    }

    public async Task<AccessState> SetDesiredAsync(Guid deviceId, string desiredValue, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO access_states (device_id, desired_value, actual_value, updated_at)
            VALUES (@deviceId, @desiredValue, 'locked', @now)
            ON CONFLICT (device_id) DO UPDATE
            SET desired_value = @desiredValue, updated_at = @now
            RETURNING device_id, desired_value, actual_value, updated_at
            """;

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("deviceId", deviceId);
        cmd.Parameters.AddWithValue("desiredValue", desiredValue);
        cmd.Parameters.AddWithValue("now", DateTimeOffset.UtcNow);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        await reader.ReadAsync(ct);
        return Read(reader);
    }

    public async Task<AccessState?> SetActualAsync(Guid deviceId, string actualValue, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE access_states
            SET actual_value = @actualValue, updated_at = @now
            WHERE device_id = @deviceId
            RETURNING device_id, desired_value, actual_value, updated_at
            """;

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("deviceId", deviceId);
        cmd.Parameters.AddWithValue("actualValue", actualValue);
        cmd.Parameters.AddWithValue("now", DateTimeOffset.UtcNow);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Read(reader) : null;
    }

    private static AccessState Read(NpgsqlDataReader reader) => new()
    {
        DeviceId = reader.GetGuid(0),
        DesiredValue = reader.GetString(1),
        ActualValue = reader.GetString(2),
        UpdatedAt = reader.GetFieldValue<DateTimeOffset>(3),
    };
}
