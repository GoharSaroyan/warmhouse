using Npgsql;

namespace DeviceManagement.Api.Infrastructure;

/// <summary>
/// Creates the Device Management service's own schema (device_types,
/// modules, devices - see docs/erd/erd.puml) if it doesn't already
/// exist. Database-per-service: this is the only service that reads or
/// writes these tables.
/// </summary>
public static class Schema
{
    public static async Task EnsureCreatedAsync(NpgsqlDataSource dataSource, CancellationToken ct = default)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS device_types (
                id UUID PRIMARY KEY,
                name VARCHAR(100) NOT NULL,
                unit VARCHAR(20) NOT NULL DEFAULT '',
                description VARCHAR(255) NOT NULL DEFAULT ''
            );

            CREATE TABLE IF NOT EXISTS modules (
                id UUID PRIMARY KEY,
                device_type_id UUID NOT NULL REFERENCES device_types(id),
                name VARCHAR(100) NOT NULL,
                manufacturer VARCHAR(100) NOT NULL DEFAULT '',
                protocol VARCHAR(50) NOT NULL DEFAULT '',
                price NUMERIC(10, 2) NOT NULL DEFAULT 0
            );

            CREATE TABLE IF NOT EXISTS devices (
                id UUID PRIMARY KEY,
                module_id UUID NOT NULL REFERENCES modules(id),
                house_id UUID NOT NULL,
                serial_number VARCHAR(100) NOT NULL,
                status VARCHAR(20) NOT NULL DEFAULT 'pending',
                installed_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE INDEX IF NOT EXISTS idx_modules_device_type_id ON modules(device_type_id);
            CREATE INDEX IF NOT EXISTS idx_devices_module_id ON devices(module_id);
            CREATE INDEX IF NOT EXISTS idx_devices_house_id ON devices(house_id);
            """;

        await using var cmd = dataSource.CreateCommand(sql);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
