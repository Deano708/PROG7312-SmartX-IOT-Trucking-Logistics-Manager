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

    public async Task<FuelRangePrediction?> PredictFuelRangeAsync(TruckTelemetry telemetry)
    {
        using var response = await _http.PostAsJsonAsync("api/fuel-prediction/range", telemetry);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<FuelRangePrediction>();
    }

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

    public Task<MeshTopologySnapshot?> GetTopologyAsync() =>
        _http.GetFromJsonAsync<MeshTopologySnapshot>("api/topology");

    public Task<MeshReachabilityResult?> ScanMeshAsync(string gatewayId) =>
        _http.GetFromJsonAsync<MeshReachabilityResult>($"api/topology/bfs-scan/{Uri.EscapeDataString(gatewayId)}");

    public Task<MeshBackboneResult?> GetMeshBackboneAsync(string startNodeId) =>
        _http.GetFromJsonAsync<MeshBackboneResult>($"api/topology/mst-backbone/{Uri.EscapeDataString(startNodeId)}");

    public Task<MeshRouteResult?> GetMeshRouteAsync(string sourceNodeId, string destinationNodeId) =>
        _http.GetFromJsonAsync<MeshRouteResult>($"api/topology/route/{Uri.EscapeDataString(sourceNodeId)}/{Uri.EscapeDataString(destinationNodeId)}");

    public Task<List<SensorTelemetryReading>?> GetHistoricalTelemetryRangeAsync(string sensorMac, DateTime fromUtc, DateTime toUtc)
    {
        var sensor = Uri.EscapeDataString(sensorMac);
        var fromString = Uri.EscapeDataString(fromUtc.ToUniversalTime().ToString("o"));
        var toString = Uri.EscapeDataString(toUtc.ToUniversalTime().ToString("o"));
        return _http.GetFromJsonAsync<List<SensorTelemetryReading>>($"api/topology/history/range?sensorMac={sensor}&fromUtc={fromString}&toUtc={toString}");
    }
}