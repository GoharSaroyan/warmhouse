using DeviceManagement.Api.Domain;

namespace DeviceManagement.Api.Application;

/// <summary>
/// The "Onboarding Handler" component from
/// docs/c4/component-device-management.puml. Runs the self-service
/// pairing flow described in docs/c4/code-device-onboarding.puml:
/// negotiate protocol config with a new device via the Device Gateway,
/// then register it - no technician visit required.
/// </summary>
public class OnboardingHandler
{
    private readonly IDeviceGatewayClient _deviceGateway;
    private readonly DeviceStateManager _stateManager;
    private readonly ILogger<OnboardingHandler> _logger;

    public OnboardingHandler(IDeviceGatewayClient deviceGateway, DeviceStateManager stateManager, ILogger<OnboardingHandler> logger)
    {
        _deviceGateway = deviceGateway;
        _stateManager = stateManager;
        _logger = logger;
    }

    public async Task<OnboardResponse> StartOnboardingAsync(OnboardRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.PairingCode))
        {
            return new OnboardResponse(false, null, "pairing_code is required");
        }

        var pairing = await _deviceGateway.NegotiateProtocolAsync(request.PairingCode, request.ModuleId, ct);
        if (!pairing.Success)
        {
            _logger.LogWarning("Onboarding failed for module {ModuleId}: {Reason}", request.ModuleId, pairing.FailureReason);
            return new OnboardResponse(false, null, pairing.FailureReason ?? "pairing failed");
        }

        try
        {
            var device = await _stateManager.RegisterDeviceAsync(
                request.ModuleId,
                request.HouseId,
                serialNumber: pairing.DeviceExternalId ?? Guid.NewGuid().ToString("N"),
                status: "active",
                ct);

            _logger.LogInformation("Device {DeviceId} onboarded for house {HouseId}", device.Id, device.HouseId);
            return new OnboardResponse(true, DeviceResponse.From(device), null);
        }
        catch (InvalidOperationException ex)
        {
            return new OnboardResponse(false, null, ex.Message);
        }
    }
}
