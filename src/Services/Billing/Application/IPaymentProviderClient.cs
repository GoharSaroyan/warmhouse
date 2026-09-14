namespace Billing.Api.Application;

public record ChargeResult(bool Success, string? TransactionId, string? FailureReason);

/// <summary>
/// Port to the external Payment Provider (docs/c4/container-to-be.puml).
/// </summary>
public interface IPaymentProviderClient
{
    Task<ChargeResult> ChargeAsync(Guid userId, string plan, CancellationToken ct = default);
}
