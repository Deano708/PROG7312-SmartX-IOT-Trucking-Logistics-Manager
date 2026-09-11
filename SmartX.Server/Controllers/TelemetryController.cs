using Microsoft.AspNetCore.Mvc;
using SmartX.Shared.Models;

namespace SmartX.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TelemetryController : ControllerBase
{
    [HttpPost("ingest")]
    public IActionResult IngestTelemetry([FromBody] TelemetryPacket<TruckTelemetry> packet)
    {
        if (string.IsNullOrEmpty(packet.DeviceMac))
            return BadRequest("Invalid device MAC address.");

        // Direct handling of non-boxed struct payload
        TruckTelemetry data = packet.Payload;

        // Log or forward to real-time command stream
        return Ok(new { Status = "Ingested", ProcessedAt = DateTime.UtcNow, Truck = data.TruckId });
    }
}