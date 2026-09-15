using Npgsql;
using Telemetry.Api.Domain;

namespace Telemetry.Api.Infrastructure;

/// <summary>Implements the read-side port; every query is capped by a limit.</summary>
public class TelemetryQueries : ITelemetryQueries
{
    private readonly NpgsqlDataSource _dataSource;

    public TelemetryQueries(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    public async Task<List<TelemetryPoint>> QueryAsync(Guid? deviceId, string? metric, DateTimeOffset? from, DateTimeOffset? to, int limit, CancellationToken ct = default)
    {
        var sql = "SELECT id, device_id, metric, value, unit, measured_at, received_at FROM telemetry_points WHERE 1=1";
        if (deviceId is not null) sql += " AND device_id = @deviceId";
        if (metric is not null) sql += " AND metric = @metric";
        if (from is not null) sql += " AND measured_at >= @from";
        if (to is not null) sql += " AND measured_at <= @to";
        sql += " ORDER BY measured_at DESC LIMIT @limit";

        await using var cmd = _dataSource.CreateCommand(sql);
        if (deviceId is not null) cmd.Parameters.AddWithValue("deviceId", deviceId.Value);
        if (metric is not null) cmd.Parameters.AddWithValue("metric", metric);
        if (from is not null) cmd.Parameters.AddWithValue("from", from.Value);
        if (to is not null) cmd.Parameters.AddWithValue("to", to.Value);
        cmd.Parameters.AddWithValue("limit", Math.Clamp(limit, 1, 1000));

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var result = new List<TelemetryPoint>();
        while (await reader.ReadAsync(ct))
        {
            result.Add(Read(reader));
        }

        return result;
    }

    public async Task<TelemetryPoint?> GetLatestAsync(Guid deviceId, string metric, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, device_id, metric, value, unit, measured_at, received_at
            FROM telemetry_points
            WHERE device_id = @deviceId AND metric = @metric
            ORDER BY measured_at DESC
            LIMIT 1
            """;

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("deviceId", deviceId);
        cmd.Parameters.AddWithValue("metric", metric);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Read(reader) : null;
    }

    private static TelemetryPoint Read(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetGuid(0),
        DeviceId = reader.GetGuid(1),
        Metric = reader.GetString(2),
        Value = reader.GetDouble(3),
        Unit = reader.GetString(4),
        MeasuredAt = reader.GetFieldValue<DateTimeOffset>(5),
        ReceivedAt = reader.GetFieldValue<DateTimeOffset>(6),
    };
}
