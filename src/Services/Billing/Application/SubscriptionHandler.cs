using Billing.Api.Domain;

namespace Billing.Api.Application;

/// <summary>
/// Owns the SaaS self-service subscription and module purchase/
/// entitlement per home.
/// </summary>
public class SubscriptionHandler
{
    private readonly ISubscriptionRepository _repository;
    private readonly IPaymentProviderClient _paymentProvider;
    private readonly ILogger<SubscriptionHandler> _logger;

    public SubscriptionHandler(ISubscriptionRepository repository, IPaymentProviderClient paymentProvider, ILogger<SubscriptionHandler> logger)
    {
        _repository = repository;
        _paymentProvider = paymentProvider;
        _logger = logger;
    }

    public Task<Subscription?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _repository.GetByIdAsync(id, ct);
    }

    public Task<List<Subscription>> GetForUserAsync(Guid userId, CancellationToken ct = default)
    {
        return _repository.GetForUserAsync(userId, ct);
    }

    public async Task<Subscription> SubscribeAsync(CreateSubscriptionRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Plan))
        {
            throw new ArgumentException("plan is required");
        }

        var charge = await _paymentProvider.ChargeAsync(request.UserId, request.Plan, ct);
        if (!charge.Success)
        {
            throw new InvalidOperationException(charge.FailureReason ?? "payment failed");
        }

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Plan = request.Plan,
            Status = "active",
            StartDate = DateTimeOffset.UtcNow,
            EndDate = null,
        };

        _logger.LogInformation("User {UserId} subscribed to {Plan} (transaction {TransactionId})", request.UserId, request.Plan, charge.TransactionId);
        return await _repository.CreateAsync(subscription, ct);
    }

    public Task<Subscription?> CancelAsync(Guid id, CancellationToken ct = default)
    {
        return _repository.UpdateStatusAsync(id, "cancelled", DateTimeOffset.UtcNow, ct);
    }
}
