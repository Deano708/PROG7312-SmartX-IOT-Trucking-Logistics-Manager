namespace SmartX.Server.Controllers;

using Microsoft.AspNetCore.Mvc;
using SmartX.Shared.Models;
using SmartX.Shared.Services;

//DELCARATION OF AI USAGE
//Gemini. (2026) . Setup of topology controller creating methods for managing mesh topology and routing. Available at:
//https://share.gemini.google/R0oOsJKGzWnJ (Accessed: 11/11/2026).


[ApiController]
[Route("api/[controller]")]
public class TopologyController : ControllerBase
{
    private readonly MeshRoutePlanner _routePlanner;
    private readonly HistoricalTelemetryIndex _historyIndex;

    public TopologyController(MeshRoutePlanner routePlanner, HistoricalTelemetryIndex historyIndex)
    {
        _routePlanner = routePlanner;
        _historyIndex = historyIndex;
    }

    [HttpGet]
    public ActionResult<MeshTopologySnapshot> GetTopology() => Ok(_routePlanner.GetSnapshot());

    [HttpGet("bfs-scan/{gatewayId}")]
    public ActionResult<MeshReachabilityResult> PerformBfsScan(string gatewayId)
    {
        try
        {
            return Ok(_routePlanner.PerformBfsReachability(gatewayId));
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Gateway '{gatewayId}' was not found.");
        }
    }

    [HttpGet("mst-backbone/{startNodeId}")]
    public ActionResult<MeshBackboneResult> GetMinimumSpanningTree(string startNodeId)
    {
        try
        {
            return Ok(_routePlanner.CalculateMinimumSpanningTreePrims(startNodeId));
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Online mesh node '{startNodeId}' was not found.");
        }
    }

    [HttpGet("route/{sourceNodeId}/{destinationNodeId}")]
    public ActionResult<MeshRouteResult> GetLowestEnergyRoute(string sourceNodeId, string destinationNodeId) =>
        Ok(_routePlanner.CalculateLowestEnergyRoute(sourceNodeId, destinationNodeId));

    [HttpGet("history/range")]
    public ActionResult<IReadOnlyList<SensorTelemetryReading>> GetHistoricalTelemetryRange(
        [FromQuery] string sensorMac,
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc)
    {
        if (string.IsNullOrWhiteSpace(sensorMac))
            return BadRequest("'sensorMac' is required.");
        if (fromUtc > toUtc)
            return BadRequest("'fromUtc' must not be later than 'toUtc'.");

        return Ok(_historyIndex.QueryRange(sensorMac, fromUtc, toUtc));
    }
}
