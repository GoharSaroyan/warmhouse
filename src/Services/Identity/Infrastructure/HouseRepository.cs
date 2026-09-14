using Identity.Api.Domain;
using Npgsql;

namespace Identity.Api.Infrastructure;

public class HouseRepository : IHouseRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public HouseRepository(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    public async Task<House?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = "SELECT id, user_id, address, created_at FROM houses WHERE id = @id";

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Read(reader) : null;
    }

    public async Task<List<House>> GetForUserAsync(Guid userId, CancellationToken ct = default)
    {
        const string sql = "SELECT id, user_id, address, created_at FROM houses WHERE user_id = @userId ORDER BY created_at";

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("userId", userId);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var result = new List<House>();
        while (await reader.ReadAsync(ct))
        {
            result.Add(Read(reader));
        }

        return result;
    }

    public async Task<House> CreateAsync(House house, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO houses (id, user_id, address, created_at)
            VALUES (@id, @userId, @address, @createdAt)
            """;

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("id", house.Id);
        cmd.Parameters.AddWithValue("userId", house.UserId);
        cmd.Parameters.AddWithValue("address", house.Address);
        cmd.Parameters.AddWithValue("createdAt", house.CreatedAt);
        await cmd.ExecuteNonQueryAsync(ct);

        return house;
    }

    private static House Read(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetGuid(0),
        UserId = reader.GetGuid(1),
        Address = reader.GetString(2),
        CreatedAt = reader.GetFieldValue<DateTimeOffset>(3),
    };
}
