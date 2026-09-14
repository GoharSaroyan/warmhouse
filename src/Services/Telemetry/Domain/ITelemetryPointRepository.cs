namespace Telemetry.Api.Domain;

public interface ITelemetryPointRepository
{
    Task<TelemetryPoint> AddAsync(TelemetryPoint point, CancellationToken ct = default);
}
