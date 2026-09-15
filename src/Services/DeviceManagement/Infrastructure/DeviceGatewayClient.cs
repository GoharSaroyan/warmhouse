using DeviceManagement.Api.Application;

namespace DeviceManagement.Api.Infrastructure;

/// <summary>
/// Stand-in adapter for docs/c4/container-to-be.puml's Device Gateway
/// container, which doesn't exist as a running service yet. Simulates a
/// pairing handshake deterministically so the onboarding endpoint is
/// testable end-to-end today.
///
/// TODO: replace with a real HTTP client call to the Device Gateway
/// service once it's built, instead of simulating the handshake here.
/// </summary>
public class DeviceGatewayClient : IDeviceGatewayClient
{
    public Task<PairingResult> NegotiateProtocolAsync(string pairingCode, Guid deviceTypeId, CancellationToken ct = default)
    {
        if (pairingCode.Equals("INVALID", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new PairingResult(false, null, null, "device unreachable or pairing code invalid"));
        }

        // A real implementation would call out to the Device Gateway,
        // which would in turn perform discovery/handshake with the
        // physical or partner device over its standard protocol.
        var externalId = $"dev-{pairingCode}-{Guid.NewGuid():N}"[..24];
        const string protocolConfig = "mqtt";

        return Task.FromResult(new PairingResult(true, externalId, protocolConfig, null));
    }
}
