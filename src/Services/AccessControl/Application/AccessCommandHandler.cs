using AccessControl.Api.Domain;
using MassTransit;
using WarmHouse.Contracts;

namespace AccessControl.Api.Application;

public record AccessStateResponse(Guid DeviceId, string DesiredValue, string ActualValue, DateTimeOffset UpdatedAt)
{
    public static AccessStateResponse From(AccessState s) => new(s.DeviceId, s.DesiredValue, s.ActualValue, s.UpdatedAt);
}

public record AccessAuditResponse(Guid Id, Guid DeviceId, string Action, string Actor, DateTimeOffset OccurredAt)
{
    public static AccessAuditResponse From(AccessAuditEntry e) => new(e.Id, e.DeviceId, e.Action, e.Actor, e.OccurredAt);
}

public record SetAccessRequest(Guid HouseId, string DesiredValue, string Actor);

/// <summary>
/// Owns locking/unlocking automatic gates and the access audit trail.
/// Every desired-state change is audited, unlike Heating/Lighting.
/// SetDesiredAsync publishes AccessCommandRequested for the Device
/// Gateway to deliver over RabbitMQ; ReportActualAsync is called by
/// AccessCommandCompletedConsumer once the ack comes back.
/// </summary>
public class AccessCommandHandler
{
    private readonly IAccessStateRepository _states;
    private readonly IAccessAuditRepository _audit;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<AccessCommandHandler> _logger;

    public AccessCommandHandler(IAccessStateRepository states, IAccessAuditRepository audit, IPublishEndpoint publishEndpoint, ILogger<AccessCommandHandler> logger)
    {
        _states = states;
        _audit = audit;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public Task<AccessState?> GetStateAsync(Guid deviceId, CancellationToken ct = default)
    {
        return _states.GetAsync(deviceId, ct);
    }

    public async Task<AccessState> SetDesiredAsync(Guid deviceId, SetAccessRequest request, CancellationToken ct = default)
    {
        var state = await _states.SetDesiredAsync(deviceId, request.DesiredValue, ct);
        await _audit.AppendAsync(deviceId, request.DesiredValue, request.Actor, ct);

        await _publishEndpoint.Publish(new AccessCommandRequested(deviceId, request.HouseId, request.DesiredValue, request.Actor), ct);
        _logger.LogInformation("Published AccessCommandRequested for device {DeviceId} -> {Value} by {Actor}", deviceId, request.DesiredValue, request.Actor);

        return state;
    }

    public Task<AccessState?> ReportActualAsync(Guid deviceId, string actualValue, CancellationToken ct = default)
    {
        return _states.SetActualAsync(deviceId, actualValue, ct);
    }

    public Task<List<AccessAuditEntry>> GetAuditTrailAsync(Guid deviceId, CancellationToken ct = default)
    {
        return _audit.GetForDeviceAsync(deviceId, ct);
    }
}
