using Npgsql;
using Telemetry.Api.Domain;

namespace Telemetry.Api.Infrastructure;

public class TelemetryPointRepository : ITelemetryPointRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public TelemetryPointRepository(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    public async Task<TelemetryPoint> AddAsync(TelemetryPoint point, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO telemetry_points (id, device_id, metric, value, unit, measured_at, received_at)
            VALUES (@id, @deviceId, @metric, @value, @unit, @measuredAt, @receivedAt)
            """;

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("id", point.Id);
        cmd.Parameters.AddWithValue("deviceId", point.DeviceId);
        cmd.Parameters.AddWithValue("metric", point.Metric);
        cmd.Parameters.AddWithValue("value", point.Value);
        cmd.Parameters.AddWithValue("unit", point.Unit);
        cmd.Parameters.AddWithValue("measuredAt", point.MeasuredAt);
        cmd.Parameters.AddWithValue("receivedAt", point.ReceivedAt);
        await cmd.ExecuteNonQueryAsync(ct);

        return point;
    }
}
