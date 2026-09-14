using System.Net.Http.Json;

namespace SmartHome.Api.Services;

/// <summary>
/// Fetches temperature readings from the external temperature-api service over HTTP.
/// </summary>
public class TemperatureService
{
    private readonly HttpClient _httpClient;

    public TemperatureService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    // GetTemperatureAsync fetches temperature data for a specific location.
    public async Task<TemperatureResponse> GetTemperatureAsync(string location, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"/temperature?location={Uri.EscapeDataString(location)}", ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TemperatureResponse>(ct)
            ?? throw new InvalidOperationException("error decoding temperature response");
    }

    // GetTemperatureByIdAsync fetches temperature data for a specific sensor ID.
    public async Task<TemperatureResponse> GetTemperatureByIdAsync(string sensorId, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"/temperature/{Uri.EscapeDataString(sensorId)}", ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TemperatureResponse>(ct)
            ?? throw new InvalidOperationException("error decoding temperature response");
    }
}
