using Npgsql;

namespace Identity.Api.Infrastructure;

public static class Schema
{
    public static async Task EnsureCreatedAsync(NpgsqlDataSource dataSource, CancellationToken ct = default)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS users (
                id UUID PRIMARY KEY,
                name VARCHAR(100) NOT NULL,
                email VARCHAR(255) NOT NULL UNIQUE,
                password_hash VARCHAR(255) NOT NULL,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS houses (
                id UUID PRIMARY KEY,
                user_id UUID NOT NULL REFERENCES users(id),
                address VARCHAR(255) NOT NULL,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE INDEX IF NOT EXISTS idx_houses_user_id ON houses(user_id);
            """;

        await using var cmd = dataSource.CreateCommand(sql);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
