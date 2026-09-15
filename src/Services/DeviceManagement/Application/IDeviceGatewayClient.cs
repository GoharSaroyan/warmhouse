namespace DeviceManagement.Api.Application;

public record PairingResult(bool Success, string? DeviceExternalId, string? ProtocolConfig, string? FailureReason);

/// <summary>
/// Port to the Device Gateway container (docs/c4/container-to-be.puml) -
/// the only thing that actually talks to physical/partner devices. The
/// onboarding flow negotiates a protocol handshake through this port
/// rather than reaching out to hardware itself.
/// </summary>
public interface IDeviceGatewayClient
{
    Task<PairingResult> NegotiateProtocolAsync(string pairingCode, Guid deviceTypeId, CancellationToken ct = default);
}
