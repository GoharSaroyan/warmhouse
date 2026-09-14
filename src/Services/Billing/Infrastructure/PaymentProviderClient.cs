using Billing.Api.Application;

namespace Billing.Api.Infrastructure;

/// <summary>
/// Stand-in adapter for the external Payment Provider
/// (docs/c4/container-to-be.puml), which this project doesn't integrate
/// with a real payment gateway. Simulates a successful charge
/// deterministically so the subscribe endpoint is testable end-to-end.
///
/// TODO: replace with a real payment gateway integration (e.g. Stripe)
/// before this goes anywhere near production.
/// </summary>
public class PaymentProviderClient : IPaymentProviderClient
{
    public Task<ChargeResult> ChargeAsync(Guid userId, string plan, CancellationToken ct = default)
    {
        if (plan.Equals("DECLINE", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new ChargeResult(false, null, "payment declined"));
        }

        return Task.FromResult(new ChargeResult(true, Guid.NewGuid().ToString("N"), null));
    }
}
