using DeviceManagement.Api.Domain;
using Npgsql;

namespace DeviceManagement.Api.Infrastructure;

public class ModuleRepository : IModuleRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public ModuleRepository(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    public async Task<List<Module>> GetAllAsync(Guid? deviceTypeId, CancellationToken ct = default)
    {
        var sql = "SELECT id, device_type_id, name, manufacturer, protocol, price FROM modules";
        if (deviceTypeId is not null)
        {
            sql += " WHERE device_type_id = @deviceTypeId";
        }
        sql += " ORDER BY name";

        await using var cmd = _dataSource.CreateCommand(sql);
        if (deviceTypeId is not null)
        {
            cmd.Parameters.AddWithValue("deviceTypeId", deviceTypeId.Value);
        }

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var result = new List<Module>();
        while (await reader.ReadAsync(ct))
        {
            result.Add(Read(reader));
        }

        return result;
    }

    public async Task<Module?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = "SELECT id, device_type_id, name, manufacturer, protocol, price FROM modules WHERE id = @id";

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Read(reader) : null;
    }

    public async Task<Module> CreateAsync(Module module, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO modules (id, device_type_id, name, manufacturer, protocol, price)
            VALUES (@id, @deviceTypeId, @name, @manufacturer, @protocol, @price)
            """;

        module.Id = Guid.NewGuid();

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("id", module.Id);
        cmd.Parameters.AddWithValue("deviceTypeId", module.DeviceTypeId);
        cmd.Parameters.AddWithValue("name", module.Name);
        cmd.Parameters.AddWithValue("manufacturer", module.Manufacturer);
        cmd.Parameters.AddWithValue("protocol", module.Protocol);
        cmd.Parameters.AddWithValue("price", module.Price);
        await cmd.ExecuteNonQueryAsync(ct);

        return module;
    }

    private static Module Read(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetGuid(0),
        DeviceTypeId = reader.GetGuid(1),
        Name = reader.GetString(2),
        Manufacturer = reader.GetString(3),
        Protocol = reader.GetString(4),
        Price = reader.GetDecimal(5),
    };
}
