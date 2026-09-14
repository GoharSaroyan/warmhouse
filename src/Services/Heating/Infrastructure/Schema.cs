using Npgsql;

namespace Heating.Api.Infrastructure;

public static class Schema
{
    public static async Task EnsureCreatedAsync(NpgsqlDataSource dataSource, CancellationToken ct = default)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS heating_states (
                device_id UUID PRIMARY KEY,
                desired_value VARCHAR(50) NOT NULL DEFAULT 'off',
                actual_value VARCHAR(50) NOT NULL DEFAULT 'off',
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            """;

        await using var cmd = dataSource.CreateCommand(sql);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
