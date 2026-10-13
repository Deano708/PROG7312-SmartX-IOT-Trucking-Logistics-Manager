using Microsoft.AspNetCore.Mvc;
using SmartX.Shared.Models;
using SmartX.Server.Services;

namespace SmartX.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TelemetryController : ControllerBase
{
    private readonly TelemetryHistoryStore _historyStore;

    public TelemetryController(TelemetryHistoryStore historyStore)
    {
        _historyStore = historyStore;
    }

    [HttpPost("ingest")]
    public IActionResult IngestTelemetry([FromBody] TelemetryPacket<TruckTelemetry> packet)
    {
        if (!IsValidPacket(packet))
            return BadRequest("Invalid telemetry packet.");

        _historyStore.Ingest(packet);
        return Ok(new { Status = "Ingested", ProcessedAt = DateTime.UtcNow, Truck = packet.Payload.TruckId });
    }

    [HttpPost("ingest/batch")]
    public IActionResult IngestTelemetryBatch([FromBody] List<TelemetryPacket<TruckTelemetry>> packets)
    {
        if (packets is null || packets.Count == 0 || packets.Any(packet => !IsValidPacket(packet)))
            return BadRequest("The telemetry batch is empty or contains an invalid packet.");

        foreach (var packet in packets)
        {
            _historyStore.Ingest(packet);
        }

        return Ok(new { Status = "Ingested", Count = packets.Count, ProcessedAt = DateTime.UtcNow });
    }

    [HttpGet("history/{truckId}")]
    public ActionResult<IReadOnlyList<TelemetryPacket<TruckTelemetry>>> GetHistory(string truckId) =>
        Ok(_historyStore.GetHistory(truckId));

    private static bool IsValidPacket(TelemetryPacket<TruckTelemetry>? packet) =>
        packet is not null &&
        !string.IsNullOrWhiteSpace(packet.DeviceMac) &&
        !string.IsNullOrWhiteSpace(packet.Payload.TruckId) &&
        double.IsFinite(packet.Payload.Latitude) &&
        packet.Payload.Latitude is >= -90 and <= 90 &&
        double.IsFinite(packet.Payload.Longitude) &&
        packet.Payload.Longitude is >= -180 and <= 180;
}