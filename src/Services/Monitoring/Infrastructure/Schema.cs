using Npgsql;

namespace Monitoring.Api.Infrastructure;

public static class Schema
{
    public static async Task EnsureCreatedAsync(NpgsqlDataSource dataSource, CancellationToken ct = default)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS live_states (
                device_id UUID PRIMARY KEY,
                house_id UUID NOT NULL,
                value VARCHAR(100) NOT NULL DEFAULT '',
                status VARCHAR(50) NOT NULL DEFAULT '',
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE INDEX IF NOT EXISTS idx_live_states_house_id ON live_states(house_id);
            """;

        await using var cmd = dataSource.CreateCommand(sql);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
