using System.Text.Json.Serialization;

// Minimal API that returns a simulated temperature reading for a given
// location or sensor, applying a default mapping between the two when
// only one of them is supplied.

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    app.Urls.Add($"http://0.0.0.0:{port}");
}

var random = new Random();

app.MapGet("/temperature", (string? location, string? sensorId) =>
    Results.Ok(BuildReading(location, sensorId, random)));

app.MapGet("/temperature/{sensorId}", (string sensorId, string? location) =>
    Results.Ok(BuildReading(location, sensorId, random)));

app.Run();

static TemperatureReading BuildReading(string? location, string? sensorId, Random random)
{
    location ??= string.Empty;
    sensorId ??= string.Empty;

    // If no location is provided, use a default based on sensor ID.
    if (string.IsNullOrEmpty(location))
    {
        location = sensorId switch
        {
            "1" => "Living Room",
            "2" => "Bedroom",
            "3" => "Kitchen",
            _ => "Unknown",
        };
    }

    // If no sensor ID is provided, generate one based on location.
    if (string.IsNullOrEmpty(sensorId))
    {
        sensorId = location switch
        {
            "Living Room" => "1",
            "Bedroom" => "2",
            "Kitchen" => "3",
            _ => "0",
        };
    }

    var value = Math.Round(random.NextDouble() * 15 + 15, 1); // 15.0 - 30.0 °C
    var status = value is >= 18 and <= 26 ? "normal" : "warning";

    return new TemperatureReading
    {
        Value = value,
        Unit = "°C",
        Timestamp = DateTimeOffset.UtcNow,
        Location = location,
        Status = status,
        SensorId = sensorId,
        SensorType = "temperature",
        Description = $"Temperature reading for {location}",
    };
}

class TemperatureReading
{
    [JsonPropertyName("value")]
    public double Value { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("sensor_id")]
    public string SensorId { get; set; } = string.Empty;

    [JsonPropertyName("sensor_type")]
    public string SensorType { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
