using Npgsql;
using SmartHome.Api.Models;

namespace SmartHome.Api.Data;

/// <summary>
/// Translated from Go's db.DB in apps/smart_home/db/db.go. NpgsqlDataSource
/// plays the role of pgxpool.Pool: a connection pool the repository queries
/// against, opened once in Program.cs and injected as a singleton.
/// </summary>
public class SensorRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public SensorRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    // GetSensorsAsync retrieves all sensors from the database.
    public async Task<List<Sensor>> GetSensorsAsync(CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, name, type, location, value, unit, status, last_updated, created_at
            FROM sensors
            ORDER BY id
            """;

        await using var cmd = _dataSource.CreateCommand(sql);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        var sensors = new List<Sensor>();
        while (await reader.ReadAsync(ct))
        {
            sensors.Add(ReadSensor(reader));
        }

        return sensors;
    }

    // GetSensorByIdAsync retrieves a sensor by its ID. Returns null if not found
    // (Go returned an error from pgx.ErrNoRows via QueryRow.Scan).
    public async Task<Sensor?> GetSensorByIdAsync(int id, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, name, type, location, value, unit, status, last_updated, created_at
            FROM sensors
            WHERE id = @id
            """;

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? ReadSensor(reader) : null;
    }

    // CreateSensorAsync creates a new sensor in the database. New sensors start
    // "inactive" with value 0, same as the Go INSERT statement.
    public async Task<Sensor> CreateSensorAsync(SensorCreate s, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO sensors (name, type, location, unit, status, last_updated, created_at)
            VALUES (@name, @type, @location, @unit, 'inactive', @now, @now)
            RETURNING id, name, type, location, value, unit, status, last_updated, created_at
            """;

        var now = DateTimeOffset.UtcNow;

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("name", s.Name);
        cmd.Parameters.AddWithValue("type", s.Type.ToString().ToLowerInvariant());
        cmd.Parameters.AddWithValue("location", s.Location);
        cmd.Parameters.AddWithValue("unit", s.Unit);
        cmd.Parameters.AddWithValue("now", now);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        await reader.ReadAsync(ct);
        return ReadSensor(reader);
    }

    // UpdateSensorAsync updates an existing sensor, building the SET clause
    // dynamically from whichever fields were provided - a direct translation
    // of the Go function's argCount / query-string building.
    public async Task<Sensor?> UpdateSensorAsync(int id, SensorUpdate s, CancellationToken ct = default)
    {
        // First check if the sensor exists, mirroring the Go GetSensorByID guard.
        if (await GetSensorByIdAsync(id, ct) is null)
        {
            return null;
        }

        var setClauses = new List<string> { "last_updated = @lastUpdated" };
        var parameters = new List<NpgsqlParameter> { new("lastUpdated", DateTimeOffset.UtcNow) };

        if (!string.IsNullOrEmpty(s.Name))
        {
            setClauses.Add("name = @name");
            parameters.Add(new NpgsqlParameter("name", s.Name));
        }

        if (s.Type is not null)
        {
            setClauses.Add("type = @type");
            parameters.Add(new NpgsqlParameter("type", s.Type.Value.ToString().ToLowerInvariant()));
        }

        if (!string.IsNullOrEmpty(s.Location))
        {
            setClauses.Add("location = @location");
            parameters.Add(new NpgsqlParameter("location", s.Location));
        }

        if (s.Value is not null)
        {
            setClauses.Add("value = @value");
            parameters.Add(new NpgsqlParameter("value", s.Value.Value));
        }

        if (!string.IsNullOrEmpty(s.Unit))
        {
            setClauses.Add("unit = @unit");
            parameters.Add(new NpgsqlParameter("unit", s.Unit));
        }

        if (!string.IsNullOrEmpty(s.Status))
        {
            setClauses.Add("status = @status");
            parameters.Add(new NpgsqlParameter("status", s.Status));
        }

        var sql = $"""
            UPDATE sensors SET {string.Join(", ", setClauses)}
            WHERE id = @id
            RETURNING id, name, type, location, value, unit, status, last_updated, created_at
            """;

        await using var cmd = _dataSource.CreateCommand(sql);
        foreach (var p in parameters)
        {
            cmd.Parameters.Add(p);
        }
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        await reader.ReadAsync(ct);
        return ReadSensor(reader);
    }

    // DeleteSensorAsync deletes a sensor by its ID. Returns false if no row
    // matched, same signal as Go's "sensor not found" error via RowsAffected.
    public async Task<bool> DeleteSensorAsync(int id, CancellationToken ct = default)
    {
        const string sql = "DELETE FROM sensors WHERE id = @id";

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("id", id);

        var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
        return rowsAffected > 0;
    }

    // UpdateSensorValueAsync updates the value and status of a sensor.
    public async Task<bool> UpdateSensorValueAsync(int id, double value, string status, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE sensors
            SET value = @value, status = @status, last_updated = @lastUpdated
            WHERE id = @id
            """;

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("value", value);
        cmd.Parameters.AddWithValue("status", status);
        cmd.Parameters.AddWithValue("lastUpdated", DateTimeOffset.UtcNow);
        cmd.Parameters.AddWithValue("id", id);

        var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);
        return rowsAffected > 0;
    }

    private static Sensor ReadSensor(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        Name = reader.GetString(1),
        Type = Enum.Parse<SensorType>(reader.GetString(2), ignoreCase: true),
        Location = reader.GetString(3),
        Value = reader.GetDouble(4),
        Unit = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
        Status = reader.GetString(6),
        LastUpdated = reader.GetFieldValue<DateTimeOffset>(7),
        CreatedAt = reader.GetFieldValue<DateTimeOffset>(8),
    };
}
