using Npgsql;

namespace Telemetry.Api.Infrastructure;

public static class Schema
{
    public static async Task EnsureCreatedAsync(NpgsqlDataSource dataSource, CancellationToken ct = default)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS telemetry_points (
                id UUID PRIMARY KEY,
                device_id UUID NOT NULL,
                metric VARCHAR(50) NOT NULL,
                value DOUBLE PRECISION NOT NULL,
                unit VARCHAR(20) NOT NULL DEFAULT '',
                measured_at TIMESTAMPTZ NOT NULL,
                received_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            -- Serves "latest N for this device/metric" reads, per
            -- docs/c4/component-telemetry.puml's TelemetryDbContext note.
            CREATE INDEX IF NOT EXISTS idx_telemetry_points_device_metric_measured_at
                ON telemetry_points (device_id, metric, measured_at DESC);

            CREATE TABLE IF NOT EXISTS threshold_rules (
                id UUID PRIMARY KEY,
                house_id UUID NOT NULL,
                device_id UUID NULL,
                metric VARCHAR(50) NOT NULL,
                operator VARCHAR(10) NOT NULL,
                value DOUBLE PRECISION NOT NULL
            );

            CREATE INDEX IF NOT EXISTS idx_threshold_rules_house_id ON threshold_rules(house_id);
            """;

        await using var cmd = dataSource.CreateCommand(sql);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
