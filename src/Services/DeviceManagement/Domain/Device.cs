namespace DeviceManagement.Api.Domain;

/// <summary>
/// A physical device paired to a home - an installed instance of a Module.
/// HouseId is a plain reference to an aggregate owned by the User Identity
/// service (database-per-service: no cross-service foreign key). See
/// docs/erd/erd.puml.
/// </summary>
public class Device
{
    public Guid Id { get; set; }
    public Guid ModuleId { get; set; }
    public Guid HouseId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public DateTimeOffset InstalledAt { get; set; }
}
