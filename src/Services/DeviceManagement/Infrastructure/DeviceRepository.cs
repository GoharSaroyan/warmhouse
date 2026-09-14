using DeviceManagement.Api.Domain;
using Npgsql;

namespace DeviceManagement.Api.Infrastructure;

public class DeviceRepository : IDeviceRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public DeviceRepository(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    public async Task<List<Device>> GetAllAsync(Guid? houseId, CancellationToken ct = default)
    {
        var sql = "SELECT id, module_id, house_id, serial_number, status, installed_at FROM devices";
        if (houseId is not null)
        {
            sql += " WHERE house_id = @houseId";
        }
        sql += " ORDER BY installed_at DESC";

        await using var cmd = _dataSource.CreateCommand(sql);
        if (houseId is not null)
        {
            cmd.Parameters.AddWithValue("houseId", houseId.Value);
        }

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var result = new List<Device>();
        while (await reader.ReadAsync(ct))
        {
            result.Add(Read(reader));
        }

        return result;
    }

    public async Task<Device?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = "SELECT id, module_id, house_id, serial_number, status, installed_at FROM devices WHERE id = @id";

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Read(reader) : null;
    }

    public async Task<Device> CreateAsync(Device device, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO devices (id, module_id, house_id, serial_number, status, installed_at)
            VALUES (@id, @moduleId, @houseId, @serialNumber, @status, @installedAt)
            """;

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("id", device.Id);
        cmd.Parameters.AddWithValue("moduleId", device.ModuleId);
        cmd.Parameters.AddWithValue("houseId", device.HouseId);
        cmd.Parameters.AddWithValue("serialNumber", device.SerialNumber);
        cmd.Parameters.AddWithValue("status", device.Status);
        cmd.Parameters.AddWithValue("installedAt", device.InstalledAt);
        await cmd.ExecuteNonQueryAsync(ct);

        return device;
    }

    public async Task<Device?> UpdateAsync(Guid id, Action<Device> apply, CancellationToken ct = default)
    {
        var device = await GetByIdAsync(id, ct);
        if (device is null)
        {
            return null;
        }

        apply(device);

        const string sql = """
            UPDATE devices
            SET serial_number = @serialNumber, status = @status, house_id = @houseId
            WHERE id = @id
            """;

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("id", device.Id);
        cmd.Parameters.AddWithValue("serialNumber", device.SerialNumber);
        cmd.Parameters.AddWithValue("status", device.Status);
        cmd.Parameters.AddWithValue("houseId", device.HouseId);
        await cmd.ExecuteNonQueryAsync(ct);

        return device;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = "DELETE FROM devices WHERE id = @id";

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("id", id);
        var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);

        return rowsAffected > 0;
    }

    private static Device Read(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetGuid(0),
        ModuleId = reader.GetGuid(1),
        HouseId = reader.GetGuid(2),
        SerialNumber = reader.GetString(3),
        Status = reader.GetString(4),
        InstalledAt = reader.GetFieldValue<DateTimeOffset>(5),
    };
}
