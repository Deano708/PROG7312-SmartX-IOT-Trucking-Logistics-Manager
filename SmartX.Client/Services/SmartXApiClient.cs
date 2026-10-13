using System.Net.Http.Json;
using SmartX.Shared.Models;

namespace SmartX.Client.Services;

public class SmartXApiClient
{
    private readonly HttpClient _http;

    public SmartXApiClient(HttpClient http)
    {
        _http = http;
    }

    // Telemetry Ingestion API call
    public async Task<bool> SendTelemetryAsync(TelemetryPacket<TruckTelemetry> packet)
    {
        var response = await _http.PostAsJsonAsync("api/telemetry/ingest", packet);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SendTelemetryBatchAsync(IReadOnlyCollection<TelemetryPacket<TruckTelemetry>> packets)
    {
        var response = await _http.PostAsJsonAsync("api/telemetry/ingest/batch", packets);
        return response.IsSuccessStatusCode;
    }

    public Task<List<TelemetryPacket<TruckTelemetry>>?> GetTelemetryHistoryAsync(string truckId) =>
        _http.GetFromJsonAsync<List<TelemetryPacket<TruckTelemetry>>>($"api/telemetry/history/{Uri.EscapeDataString(truckId)}");

    public Task<List<FleetAlert>?> GetEmergencyAlertsAsync(CancellationToken cancellationToken = default) =>
        _http.GetFromJsonAsync<List<FleetAlert>>("api/telemetry/alerts", cancellationToken);

    // Multipart File Upload API call
    public async Task<bool> UploadReceiptAsync(string truckId, Stream fileStream, string fileName)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        content.Add(streamContent, "file", fileName);

        var response = await _http.PostAsync($"api/media/upload/{truckId}", content);
        return response.IsSuccessStatusCode;
    }

    // Account API calls
    public async Task<bool> RegisterUserAsync(string company, string email, string password)
    {
        var response = await _http.PostAsJsonAsync("api/account/register", new { CompanyName = company, Email = email, Password = password });
        return response.IsSuccessStatusCode;
    }
}