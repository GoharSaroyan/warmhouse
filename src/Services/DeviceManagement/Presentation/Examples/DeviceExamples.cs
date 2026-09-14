using DeviceManagement.Api.Application;
using Swashbuckle.AspNetCore.Filters;

namespace DeviceManagement.Api.Presentation.Examples;

/// <summary>Example bodies shown in Swagger for the device CRUD endpoints (Task 4, section 2).</summary>
public class DeviceCreateRequestExample : IExamplesProvider<DeviceCreateRequest>
{
    public DeviceCreateRequest GetExamples() => new(
        ModuleId: Guid.Parse("b3a8509f-92c0-458d-9ff5-5c0c1b35efb8"),
        HouseId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
        SerialNumber: "SN-0001");
}

public class DeviceUpdateRequestExample : IExamplesProvider<DeviceUpdateRequest>
{
    public DeviceUpdateRequest GetExamples() => new(
        SerialNumber: null,
        Status: "inactive",
        HouseId: null);
}

public class DeviceResponseExample : IExamplesProvider<DeviceResponse>
{
    public DeviceResponse GetExamples() => new(
        Id: Guid.Parse("da407758-bb22-4851-b5f6-4ed370d3d8ca"),
        ModuleId: Guid.Parse("b3a8509f-92c0-458d-9ff5-5c0c1b35efb8"),
        HouseId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
        SerialNumber: "SN-0001",
        Status: "active",
        InstalledAt: DateTimeOffset.Parse("2026-09-14T15:14:51.838Z"));
}

public class DeviceListResponseExample : IExamplesProvider<List<DeviceResponse>>
{
    public List<DeviceResponse> GetExamples() => [new DeviceResponseExample().GetExamples()];
}

public class OnboardRequestExample : IExamplesProvider<OnboardRequest>
{
    public OnboardRequest GetExamples() => new(
        HouseId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
        ModuleId: Guid.Parse("b3a8509f-92c0-458d-9ff5-5c0c1b35efb8"),
        PairingCode: "ABC123");
}

public class OnboardResponseSuccessExample : IExamplesProvider<OnboardResponse>
{
    public OnboardResponse GetExamples() => new(
        Success: true,
        Device: new DeviceResponseExample().GetExamples(),
        Error: null);
}

/// <summary>The failure shape returned when the pairing code is rejected or the device is unreachable.</summary>
public class OnboardResponseFailureExample : IExamplesProvider<OnboardResponse>
{
    public OnboardResponse GetExamples() => new(
        Success: false,
        Device: null,
        Error: "device unreachable or pairing code invalid");
}

/// <summary>The shape of every error body this service returns (400/404).</summary>
public class ErrorExample : IExamplesProvider<object>
{
    public object GetExamples() => new { error = "Device not found" };
}
