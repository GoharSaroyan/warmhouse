using Npgsql;
using Telemetry.Api.Domain;

namespace Telemetry.Api.Infrastructure;

public class ThresholdRuleRepository : IThresholdRuleRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public ThresholdRuleRepository(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    public async Task<List<ThresholdRule>> GetApplicableRulesAsync(Guid houseId, Guid deviceId, string metric, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, house_id, device_id, metric, operator, value
            FROM threshold_rules
            WHERE house_id = @houseId
              AND metric = @metric
              AND (device_id IS NULL OR device_id = @deviceId)
            """;

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("houseId", houseId);
        cmd.Parameters.AddWithValue("deviceId", deviceId);
        cmd.Parameters.AddWithValue("metric", metric);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var result = new List<ThresholdRule>();
        while (await reader.ReadAsync(ct))
        {
            result.Add(Read(reader));
        }

        return result;
    }

    public async Task<List<ThresholdRule>> GetForHouseAsync(Guid houseId, CancellationToken ct = default)
    {
        const string sql = "SELECT id, house_id, device_id, metric, operator, value FROM threshold_rules WHERE house_id = @houseId";

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("houseId", houseId);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var result = new List<ThresholdRule>();
        while (await reader.ReadAsync(ct))
        {
            result.Add(Read(reader));
        }

        return result;
    }

    public async Task<ThresholdRule> CreateAsync(ThresholdRule rule, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO threshold_rules (id, house_id, device_id, metric, operator, value)
            VALUES (@id, @houseId, @deviceId, @metric, @operator, @value)
            """;

        rule.Id = Guid.NewGuid();

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("id", rule.Id);
        cmd.Parameters.AddWithValue("houseId", rule.HouseId);
        cmd.Parameters.AddWithValue("deviceId", (object?)rule.DeviceId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("metric", rule.Metric);
        cmd.Parameters.AddWithValue("operator", rule.Operator);
        cmd.Parameters.AddWithValue("value", rule.Value);
        await cmd.ExecuteNonQueryAsync(ct);

        return rule;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = "DELETE FROM threshold_rules WHERE id = @id";

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("id", id);
        var rowsAffected = await cmd.ExecuteNonQueryAsync(ct);

        return rowsAffected > 0;
    }

    private static ThresholdRule Read(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetGuid(0),
        HouseId = reader.GetGuid(1),
        DeviceId = reader.IsDBNull(2) ? null : reader.GetGuid(2),
        Metric = reader.GetString(3),
        Operator = reader.GetString(4),
        Value = reader.GetDouble(5),
    };
}
