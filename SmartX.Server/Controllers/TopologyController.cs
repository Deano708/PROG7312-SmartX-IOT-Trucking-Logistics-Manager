namespace SmartX.Server.Controllers;

using Microsoft.AspNetCore.Mvc;
using SmartX.Shared.Models;
using SmartX.Shared.Services;

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

    /// <summary>
    /// Performs a Breadth-First Search (BFS) reachability scan starting from the gateway node.
    /// </summary>
    [HttpGet("bfs-scan/{gatewayId}")]
    public IActionResult PerformBfsScan(string gatewayId)
    {
        var (reachableNodes, isolatedNodes) = _routePlanner.PerformBfsReachability(gatewayId);
        return Ok(new
        {
            GatewayId = gatewayId,
            ReachableCount = reachableNodes.Count,
            IsolatedCount = isolatedNodes.Count,
            ReachableNodes = reachableNodes,
            IsolatedNodes = isolatedNodes,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Calculates Prim's Minimum Spanning Tree (MST) to determine the power-efficient routing backbone.
    /// </summary>
    [HttpGet("mst-backbone/{startNodeId}")]
    public IActionResult GetMinimumSpanningTree(string startNodeId)
    {
        var mstLinks = _routePlanner.CalculateMinimumSpanningTreePrims(startNodeId);
        double totalCost = mstLinks.Sum(l => l.LinkCost);

        return Ok(new
        {
            StartNodeId = startNodeId,
            TotalHops = mstLinks.Count,
            TotalCumulativeCost = Math.Round(totalCost, 2),
            MstEdges = mstLinks
        });
    }

    /// <summary>
    /// Queries historical telemetry records within a UTC timestamp range using the Red-Black Tree index.
    /// </summary>
    [HttpGet("history/range")]
    public IActionResult GetHistoricalTelemetryRange([FromQuery] DateTime fromUtc, [FromQuery] DateTime toUtc)
    {
        if (fromUtc >= toUtc)
        {
            return BadRequest("'fromUtc' timestamp must be strictly earlier than 'toUtc'.");
        }

        var records = _historyIndex.QueryRange(fromUtc, toUtc);
        return Ok(records);
    }
}