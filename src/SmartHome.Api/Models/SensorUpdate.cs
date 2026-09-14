using System.Text.Json.Serialization;

namespace SmartHome.Api.Models;

/// <summary>
/// The data that can be updated for a sensor. Fields left null are not
/// changed - see SensorRepository.UpdateSensorAsync.
/// </summary>
public class SensorUpdate
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("type")]
    public SensorType? Type { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("value")]
    public double? Value { get; set; }

    [JsonPropertyName("unit")]
    public string? Unit { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}
