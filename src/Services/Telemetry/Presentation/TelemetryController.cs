using Microsoft.AspNetCore.Mvc;
using Telemetry.Api.Application;
using Telemetry.Api.Domain;

namespace Telemetry.Api.Presentation;

/// <summary>Reached via the API Gateway at /api/v1/telemetry.</summary>
[ApiController]
[Route("")]
public class TelemetryController : ControllerBase
{
    private readonly RecordMeasurementHandler _record;
    private readonly ITelemetryQueries _queries;

    public TelemetryController(RecordMeasurementHandler record, ITelemetryQueries queries)
    {
        _record = record;
        _queries = queries;
    }

    // POST /api/v1/telemetry - manual/backfill ingest. The normal path is
    // Device Gateway -> Message Broker -> TelemetryReportedConsumer, once
    // that exists (see docs/c4/component-telemetry.puml).
    [HttpPost]
    public async Task<ActionResult<TelemetryPointResponse>> Ingest([FromBody] IngestRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Metric))
        {
            return BadRequest(new { error = "metric is required" });
        }

        var point = await _record.HandleAsync(request, ct);
        return Ok(TelemetryPointResponse.From(point));
    }

    // GET /api/v1/telemetry?deviceId=...&metric=...&from=...&to=...&limit=...
    [HttpGet]
    public async Task<ActionResult<List<TelemetryPointResponse>>> Query(
        [FromQuery] Guid? deviceId, [FromQuery] string? metric,
        [FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to,
        [FromQuery] int limit = 100, CancellationToken ct = default)
    {
        var points = await _queries.QueryAsync(deviceId, metric, from, to, limit, ct);
        return Ok(points.Select(TelemetryPointResponse.From));
    }

    // GET /api/v1/telemetry/latest?deviceId=...&metric=...
    [HttpGet("latest")]
    public async Task<ActionResult<TelemetryPointResponse>> GetLatest([FromQuery] Guid deviceId, [FromQuery] string metric, CancellationToken ct)
    {
        var point = await _queries.GetLatestAsync(deviceId, metric, ct);
        return point is null ? NotFound(new { error = "No telemetry for this device/metric" }) : Ok(TelemetryPointResponse.From(point));
    }
}
