namespace Billing.Api.Domain;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Subscription>> GetForUserAsync(Guid userId, CancellationToken ct = default);
    Task<Subscription> CreateAsync(Subscription subscription, CancellationToken ct = default);
    Task<Subscription?> UpdateStatusAsync(Guid id, string status, DateTimeOffset? endDate, CancellationToken ct = default);
}
