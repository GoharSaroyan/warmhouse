using AccessControl.Api.Domain;
using Npgsql;

namespace AccessControl.Api.Infrastructure;

public class AccessAuditRepository : IAccessAuditRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public AccessAuditRepository(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    public async Task<AccessAuditEntry> AppendAsync(Guid deviceId, string action, string actor, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO access_audit_log (id, device_id, action, actor, occurred_at)
            VALUES (@id, @deviceId, @action, @actor, @now)
            """;

        var entry = new AccessAuditEntry
        {
            Id = Guid.NewGuid(),
            DeviceId = deviceId,
            Action = action,
            Actor = actor,
            OccurredAt = DateTimeOffset.UtcNow,
        };

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("id", entry.Id);
        cmd.Parameters.AddWithValue("deviceId", entry.DeviceId);
        cmd.Parameters.AddWithValue("action", entry.Action);
        cmd.Parameters.AddWithValue("actor", entry.Actor);
        cmd.Parameters.AddWithValue("now", entry.OccurredAt);
        await cmd.ExecuteNonQueryAsync(ct);

        return entry;
    }

    public async Task<List<AccessAuditEntry>> GetForDeviceAsync(Guid deviceId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, device_id, action, actor, occurred_at
            FROM access_audit_log
            WHERE device_id = @deviceId
            ORDER BY occurred_at DESC
            """;

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("deviceId", deviceId);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var result = new List<AccessAuditEntry>();
        while (await reader.ReadAsync(ct))
        {
            result.Add(new AccessAuditEntry
            {
                Id = reader.GetGuid(0),
                DeviceId = reader.GetGuid(1),
                Action = reader.GetString(2),
                Actor = reader.GetString(3),
                OccurredAt = reader.GetFieldValue<DateTimeOffset>(4),
            });
        }

        return result;
    }
}
