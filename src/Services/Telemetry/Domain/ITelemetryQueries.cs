namespace Telemetry.Api.Domain;

/// <summary>Read-side port - projects the time series into DTOs. Every query is capped by a limit.</summary>
public interface ITelemetryQueries
{
    Task<List<TelemetryPoint>> QueryAsync(Guid? deviceId, string? metric, DateTimeOffset? from, DateTimeOffset? to, int limit, CancellationToken ct = default);
    Task<TelemetryPoint?> GetLatestAsync(Guid deviceId, string metric, CancellationToken ct = default);
}
