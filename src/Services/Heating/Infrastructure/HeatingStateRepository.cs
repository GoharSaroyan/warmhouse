using Heating.Api.Domain;
using Npgsql;

namespace Heating.Api.Infrastructure;

public class HeatingStateRepository : IHeatingStateRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public HeatingStateRepository(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    public async Task<HeatingState?> GetAsync(Guid deviceId, CancellationToken ct = default)
    {
        const string sql = "SELECT device_id, desired_value, actual_value, updated_at FROM heating_states WHERE device_id = @deviceId";

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("deviceId", deviceId);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Read(reader) : null;
    }

    public async Task<HeatingState> SetDesiredAsync(Guid deviceId, string desiredValue, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO heating_states (device_id, desired_value, actual_value, updated_at)
            VALUES (@deviceId, @desiredValue, 'off', @now)
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

    public async Task<HeatingState?> SetActualAsync(Guid deviceId, string actualValue, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE heating_states
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

    private static HeatingState Read(NpgsqlDataReader reader) => new()
    {
        DeviceId = reader.GetGuid(0),
        DesiredValue = reader.GetString(1),
        ActualValue = reader.GetString(2),
        UpdatedAt = reader.GetFieldValue<DateTimeOffset>(3),
    };
}
