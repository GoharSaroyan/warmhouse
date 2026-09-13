using System.Text.Json.Serialization;

namespace SmartHome.Api.Models;

/// <summary>
/// Translated from Go's SensorUpdate struct. Go used the zero value ("" / nil)
/// of each field to mean "not provided"; here that's expressed with nullable
/// properties, checked the same way in SensorRepository.UpdateSensorAsync.
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
