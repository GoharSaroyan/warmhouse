namespace AccessControl.Api.Domain;

/// <summary>
/// Desired vs. actual gate state for one device. See docs/erd/erd.puml's
/// AccessState entity.
/// </summary>
public class AccessState
{
    public Guid DeviceId { get; set; }
    public string DesiredValue { get; set; } = "locked";
    public string ActualValue { get; set; } = "locked";
    public DateTimeOffset UpdatedAt { get; set; }
}

/// <summary>
/// An audit trail entry for a gate action - required because access
/// control has safety/security requirements heating and lighting don't
/// (see Task 1's domain write-up).
/// </summary>
public class AccessAuditEntry
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
}
