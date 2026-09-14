using Billing.Api.Domain;

namespace Billing.Api.Application;

public record SubscriptionResponse(Guid Id, Guid UserId, string Plan, string Status, DateTimeOffset StartDate, DateTimeOffset? EndDate)
{
    public static SubscriptionResponse From(Subscription s) => new(s.Id, s.UserId, s.Plan, s.Status, s.StartDate, s.EndDate);
}

public record CreateSubscriptionRequest(Guid UserId, string Plan);

public record UpdateSubscriptionRequest(string Status);
