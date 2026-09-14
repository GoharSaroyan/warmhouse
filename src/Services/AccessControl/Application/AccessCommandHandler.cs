using AccessControl.Api.Domain;

namespace AccessControl.Api.Application;

public record AccessStateResponse(Guid DeviceId, string DesiredValue, string ActualValue, DateTimeOffset UpdatedAt)
{
    public static AccessStateResponse From(AccessState s) => new(s.DeviceId, s.DesiredValue, s.ActualValue, s.UpdatedAt);
}

public record AccessAuditResponse(Guid Id, Guid DeviceId, string Action, string Actor, DateTimeOffset OccurredAt)
{
    public static AccessAuditResponse From(AccessAuditEntry e) => new(e.Id, e.DeviceId, e.Action, e.Actor, e.OccurredAt);
}

public record SetAccessRequest(string DesiredValue, string Actor);

/// <summary>
/// Owns locking/unlocking automatic gates and the access audit trail.
/// Every desired-state change is audited, unlike Heating/Lighting.
/// </summary>
public class AccessCommandHandler
{
    private readonly IAccessStateRepository _states;
    private readonly IAccessAuditRepository _audit;
    private readonly ILogger<AccessCommandHandler> _logger;

    public AccessCommandHandler(IAccessStateRepository states, IAccessAuditRepository audit, ILogger<AccessCommandHandler> logger)
    {
        _states = states;
        _audit = audit;
        _logger = logger;
    }

    public Task<AccessState?> GetStateAsync(Guid deviceId, CancellationToken ct = default)
    {
        return _states.GetAsync(deviceId, ct);
    }

    public async Task<AccessState> SetDesiredAsync(Guid deviceId, string desiredValue, string actor, CancellationToken ct = default)
    {
        var state = await _states.SetDesiredAsync(deviceId, desiredValue, ct);
        await _audit.AppendAsync(deviceId, desiredValue, actor, ct);
        _logger.LogInformation("Gate {DeviceId} set to {Value} by {Actor}", deviceId, desiredValue, actor);
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
