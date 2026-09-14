namespace Billing.Api.Domain;

/// <summary>
/// The SaaS self-service subscription and module purchase/entitlement
/// per home. One user can have many subscriptions over time. See
/// docs/erd/erd.puml.
/// </summary>
public class Subscription
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Plan { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
}
