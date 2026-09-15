namespace DeviceManagement.Api.Domain;

/// <summary>
/// A purchasable device product (e.g. "SmartTherm X1") that homeowners buy
/// and later pair as one or more physical Devices. See docs/erd/erd.puml.
/// </summary>
public class Module
{
    public Guid Id { get; set; }
    public Guid DeviceTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Protocol { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
