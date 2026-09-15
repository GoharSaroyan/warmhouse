namespace AccessControl.Api.Domain;

public interface IAccessAuditRepository
{
    Task<AccessAuditEntry> AppendAsync(Guid deviceId, string action, string actor, CancellationToken ct = default);
    Task<List<AccessAuditEntry>> GetForDeviceAsync(Guid deviceId, CancellationToken ct = default);
}
