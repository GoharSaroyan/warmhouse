using Billing.Api.Domain;
using Npgsql;

namespace Billing.Api.Infrastructure;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public SubscriptionRepository(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    public async Task<Subscription?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = "SELECT id, user_id, plan, status, start_date, end_date FROM subscriptions WHERE id = @id";

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Read(reader) : null;
    }

    public async Task<List<Subscription>> GetForUserAsync(Guid userId, CancellationToken ct = default)
    {
        const string sql = "SELECT id, user_id, plan, status, start_date, end_date FROM subscriptions WHERE user_id = @userId ORDER BY start_date DESC";

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("userId", userId);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var result = new List<Subscription>();
        while (await reader.ReadAsync(ct))
        {
            result.Add(Read(reader));
        }

        return result;
    }

    public async Task<Subscription> CreateAsync(Subscription subscription, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO subscriptions (id, user_id, plan, status, start_date, end_date)
            VALUES (@id, @userId, @plan, @status, @startDate, @endDate)
            """;

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("id", subscription.Id);
        cmd.Parameters.AddWithValue("userId", subscription.UserId);
        cmd.Parameters.AddWithValue("plan", subscription.Plan);
        cmd.Parameters.AddWithValue("status", subscription.Status);
        cmd.Parameters.AddWithValue("startDate", subscription.StartDate);
        cmd.Parameters.AddWithValue("endDate", (object?)subscription.EndDate ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(ct);

        return subscription;
    }

    public async Task<Subscription?> UpdateStatusAsync(Guid id, string status, DateTimeOffset? endDate, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE subscriptions
            SET status = @status, end_date = @endDate
            WHERE id = @id
            RETURNING id, user_id, plan, status, start_date, end_date
            """;

        await using var cmd = _dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("status", status);
        cmd.Parameters.AddWithValue("endDate", (object?)endDate ?? DBNull.Value);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Read(reader) : null;
    }

    private static Subscription Read(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetGuid(0),
        UserId = reader.GetGuid(1),
        Plan = reader.GetString(2),
        Status = reader.GetString(3),
        StartDate = reader.GetFieldValue<DateTimeOffset>(4),
        EndDate = reader.IsDBNull(5) ? null : reader.GetFieldValue<DateTimeOffset>(5),
    };
}
