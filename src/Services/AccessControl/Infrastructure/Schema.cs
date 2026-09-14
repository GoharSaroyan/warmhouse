using Npgsql;

namespace AccessControl.Api.Infrastructure;

public static class Schema
{
    public static async Task EnsureCreatedAsync(NpgsqlDataSource dataSource, CancellationToken ct = default)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS access_states (
                device_id UUID PRIMARY KEY,
                desired_value VARCHAR(50) NOT NULL DEFAULT 'locked',
                actual_value VARCHAR(50) NOT NULL DEFAULT 'locked',
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS access_audit_log (
                id UUID PRIMARY KEY,
                device_id UUID NOT NULL,
                action VARCHAR(50) NOT NULL,
                actor VARCHAR(100) NOT NULL,
                occurred_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE INDEX IF NOT EXISTS idx_access_audit_device_id ON access_audit_log(device_id);
            """;

        await using var cmd = dataSource.CreateCommand(sql);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
