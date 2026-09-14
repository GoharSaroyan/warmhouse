using Microsoft.AspNetCore.Mvc;
using SmartHome.Api.Data;
using SmartHome.Api.Models;
using SmartHome.Api.Services;

namespace SmartHome.Api.Controllers;

/// <summary>
/// Handles sensor-related requests under /api/v1/sensors.
/// </summary>
[ApiController]
[Route("api/v1/sensors")]
public class SensorsController : ControllerBase
{
    private readonly SensorRepository _repository;
    private readonly TemperatureService _temperatureService;
    private readonly ILogger<SensorsController> _logger;

    public SensorsController(SensorRepository repository, TemperatureService temperatureService, ILogger<SensorsController> logger)
    {
        _repository = repository;
        _temperatureService = temperatureService;
        _logger = logger;
    }

    // GET /api/v1/sensors
    [HttpGet]
    public async Task<ActionResult<List<Sensor>>> GetSensors(CancellationToken ct)
    {
        var sensors = await _repository.GetSensorsAsync(ct);

        // Update temperature sensors with real-time data from the external API.
        foreach (var sensor in sensors.Where(s => s.Type == SensorType.Temperature))
        {
            try
            {
                var tempData = await _temperatureService.GetTemperatureByIdAsync(sensor.Id.ToString(), ct);
                sensor.Value = tempData.Value;
                sensor.Status = tempData.Status;
                sensor.LastUpdated = tempData.Timestamp;
                _logger.LogInformation("Updated temperature data for sensor {SensorId} from external API", sensor.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch temperature data for sensor {SensorId}", sensor.Id);
            }
        }

        return Ok(sensors);
    }

    // GET /api/v1/sensors/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Sensor>> GetSensorById(int id, CancellationToken ct)
    {
        var sensor = await _repository.GetSensorByIdAsync(id, ct);
        if (sensor is null)
        {
            return NotFound(new { error = "Sensor not found" });
        }

        // If this is a temperature sensor, fetch real-time data from the temperature API.
        if (sensor.Type == SensorType.Temperature)
        {
            try
            {
                var tempData = await _temperatureService.GetTemperatureByIdAsync(sensor.Id.ToString(), ct);
                sensor.Value = tempData.Value;
                sensor.Status = tempData.Status;
                sensor.LastUpdated = tempData.Timestamp;
                _logger.LogInformation("Updated temperature data for sensor {SensorId} from external API", sensor.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch temperature data for sensor {SensorId}", sensor.Id);
            }
        }

        return Ok(sensor);
    }

    // GET /api/v1/sensors/temperature/{location}
    [HttpGet("temperature/{location}")]
    public async Task<IActionResult> GetTemperatureByLocation(string location, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            return BadRequest(new { error = "Location is required" });
        }

        try
        {
            var tempData = await _temperatureService.GetTemperatureAsync(location, ct);
            return Ok(new
            {
                location = tempData.Location,
                value = tempData.Value,
                unit = tempData.Unit,
                status = tempData.Status,
                timestamp = tempData.Timestamp,
                description = tempData.Description,
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = $"Failed to fetch temperature data: {ex.Message}" });
        }
    }

    // POST /api/v1/sensors
    [HttpPost]
    public async Task<ActionResult<Sensor>> CreateSensor([FromBody] SensorCreate sensorCreate, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { error = "invalid request body" });
        }

        var sensor = await _repository.CreateSensorAsync(sensorCreate, ct);
        return CreatedAtAction(nameof(GetSensorById), new { id = sensor.Id }, sensor);
    }

    // PUT /api/v1/sensors/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Sensor>> UpdateSensor(int id, [FromBody] SensorUpdate sensorUpdate, CancellationToken ct)
    {
        var sensor = await _repository.UpdateSensorAsync(id, sensorUpdate, ct);
        if (sensor is null)
        {
            return NotFound(new { error = "Sensor not found" });
        }

        return Ok(sensor);
    }

    // DELETE /api/v1/sensors/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteSensor(int id, CancellationToken ct)
    {
        var deleted = await _repository.DeleteSensorAsync(id, ct);
        if (!deleted)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "sensor not found" });
        }

        return Ok(new { message = "Sensor deleted successfully" });
    }

    public class UpdateSensorValueRequest
    {
        public double Value { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    // PATCH /api/v1/sensors/{id}/value
    [HttpPatch("{id:int}/value")]
    public async Task<IActionResult> UpdateSensorValue(int id, [FromBody] UpdateSensorValueRequest request, CancellationToken ct)
    {
        var updated = await _repository.UpdateSensorValueAsync(id, request.Value, request.Status, ct);
        if (!updated)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "sensor not found" });
        }

        return Ok(new { message = "Sensor value updated successfully" });
    }
}
