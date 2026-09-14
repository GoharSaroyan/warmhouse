using System.Text.Json.Serialization;

namespace SmartHome.Api.Models;

/// <summary>
/// The type of sensor. Serializes as a lowercase JSON string.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<SensorType>))]
public enum SensorType
{
    [JsonStringEnumMemberName("temperature")]
    Temperature
}
