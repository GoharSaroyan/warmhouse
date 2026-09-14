using DeviceManagement.Api.Domain;
using Npgsql;

namespace DeviceManagement.Api.Infrastructure;

public class DeviceTypeRepository : IDeviceTypeRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public DeviceTypeRepository(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    public async Task<List<DeviceType>> GetAllAsync(CancellationToken ct = default)
    {
        const string sql = "SELECT id, name, unit, description FROM device_types ORDER BY name";

        await using var cmd = _dataSource.CreateCommand(sql);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        var result = new List<DeviceType>();
        while (await reader.ReadAsync(ct))
        {
            result.Add(Read(reader));
        }

        return result;
    }

    public async Task<DeviceType?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = "SELECT id, name, unit, description FROM device_types WHERE id = @id";

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Read(reader) : null;
    }

    public async Task<DeviceType> CreateAsync(DeviceType deviceType, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO device_types (id, name, unit, description)
            VALUES (@id, @name, @unit, @description)
            """;

        deviceType.Id = Guid.NewGuid();

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("id", deviceType.Id);
        cmd.Parameters.AddWithValue("name", deviceType.Name);
        cmd.Parameters.AddWithValue("unit", deviceType.Unit);
        cmd.Parameters.AddWithValue("description", deviceType.Description);
        await cmd.ExecuteNonQueryAsync(ct);

        return deviceType;
    }

    private static DeviceType Read(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetGuid(0),
        Name = reader.GetString(1),
        Unit = reader.GetString(2),
        Description = reader.GetString(3),
    };
}
