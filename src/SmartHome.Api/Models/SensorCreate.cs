using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartHome.Api.Models;

/// <summary>
/// Translated from Go's SensorCreate struct. The `binding:"required"` tags
/// become [Required] data annotations, validated via ModelState in the controller.
/// </summary>
public class SensorCreate
{
    [JsonPropertyName("name")]
    [Required]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    [Required]
    public SensorType Type { get; set; }

    [JsonPropertyName("location")]
    [Required]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;
}
