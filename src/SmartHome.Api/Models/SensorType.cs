using System.Text.Json.Serialization;

namespace SmartHome.Api.Models;

/// <summary>
/// Translated from Go's SensorType string constant in apps/smart_home/models/sensor.go.
/// Serializes to/from the same lowercase JSON string values.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<SensorType>))]
public enum SensorType
{
    [JsonStringEnumMemberName("temperature")]
    Temperature
}
