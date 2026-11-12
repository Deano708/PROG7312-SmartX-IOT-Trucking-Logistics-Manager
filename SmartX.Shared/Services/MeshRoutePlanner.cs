namespace SmartX.Shared.Services;

using SmartX.Shared.Models;

//DELCARATION OF AI USAGE
//Gemini. (2026) . Setup of MeshRoutePlanner for planning routes in the mesh network & calculating minimum spanning tree & lowest route cost. Available at: 
//https://share.gemini.google/R0oOsJKGzWnJ (Accessed: 11/11/2026).

public class MeshRoutePlanner
{
    private readonly Dictionary<string, MeshNode> _nodes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<MeshLink>> _adjacencyList = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _sync = new();

    public MeshRoutePlanner()
    {
        var locations = new (double Latitude, double Longitude)[]
        {
            (-29.8587, 31.0218), (-26.2041, 28.0473), (-33.9249, 18.4241), (-33.9608, 25.6022),
            (-23.9045, 29.4688), (-29.1181, 26.2243), (-25.6544, 27.2559), (-33.0153, 27.8935),
            (-25.4753, 30.9694), (-28.7282, 24.7499), (-28.7807, 32.0383), (-28.4478, 21.2561)
        };

        RegisterNode(new MeshNode("GW-01", "", "Fleet Gateway", -28.5, 28.5, true, true, 100));
        for (var index = 0; index < locations.Length; index++)
        {
            var truckNumber = index + 1;
            var nodeId = SensorMac(truckNumber);
            var location = locations[index];
            RegisterNode(new MeshNode(nodeId, $"TRK-{truckNumber:000}", "ESP32 GPS / telemetry", location.Latitude, location.Longitude, false, truckNumber != 11, 100 - truckNumber * 3));
        }

        AddDemoLink("GW-01", SensorMac(1), 0.8);
        AddDemoLink("GW-01", SensorMac(2), 1.1);
        AddDemoLink("GW-01", SensorMac(5), 1.4);
        AddDemoLink(SensorMac(1), SensorMac(2), 0.6);
        AddDemoLink(SensorMac(2), SensorMac(3), 0.9);
        AddDemoLink(SensorMac(2), SensorMac(4), 1.2);
        AddDemoLink(SensorMac(4), SensorMac(5), 0.7);
        AddDemoLink(SensorMac(5), SensorMac(6), 0.8);
        AddDemoLink(SensorMac(6), SensorMac(7), 0.6);
        AddDemoLink(SensorMac(7), SensorMac(8), 0.9);
        AddDemoLink(SensorMac(8), SensorMac(9), 0.8);
        AddDemoLink(SensorMac(9), SensorMac(10), 0.7);
        AddDemoLink(SensorMac(10), SensorMac(11), 0.8);
        AddDemoLink(SensorMac(11), SensorMac(12), 1.0);
        AddDemoLink(SensorMac(3), SensorMac(6), 1.3);
        AddDemoLink(SensorMac(4), SensorMac(8), 1.1);
        AddDemoLink(SensorMac(7), SensorMac(10), 1.2);
        AddDemoLink(SensorMac(9), SensorMac(12), 1.4);
    }

    public static string SensorMac(int truckNumber) => $"02:42:AC:11:00:{truckNumber:00}";

    public void RegisterNode(MeshNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (string.IsNullOrWhiteSpace(node.NodeId))
            throw new ArgumentException("A mesh node must have an ID.", nameof(node));

        lock (_sync)
        {
            _nodes[node.NodeId] = node;
            _adjacencyList.TryAdd(node.NodeId, new List<MeshLink>());
        }
    }

    public void AddLink(MeshLink link)
    {
        ArgumentNullException.ThrowIfNull(link);
        if (!double.IsFinite(link.LinkCost) || link.LinkCost <= 0 || !double.IsFinite(link.RssiDbm) ||
            !double.IsFinite(link.PacketLossRate) || link.PacketLossRate is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(link), "Link cost, signal strength, and packet loss must be finite and valid.");

        lock (_sync)
        {
            if (!_nodes.ContainsKey(link.SourceNodeId) || !_nodes.ContainsKey(link.TargetNodeId))
                throw new ArgumentException("Both link endpoints must be registered mesh nodes.", nameof(link));

            if (_adjacencyList[link.SourceNodeId].Any(existing => string.Equals(existing.TargetNodeId, link.TargetNodeId, StringComparison.OrdinalIgnoreCase)))
                return;

            _adjacencyList[link.SourceNodeId].Add(link);
            _adjacencyList[link.TargetNodeId].Add(link with
            {
                SourceNodeId = link.TargetNodeId,
                TargetNodeId = link.SourceNodeId
            });
        }
    }

    public MeshTopologySnapshot GetSnapshot()
    {
        lock (_sync)
        {
            var links = _adjacencyList.Values.SelectMany(value => value)
                .Where(link => string.Compare(link.SourceNodeId, link.TargetNodeId, StringComparison.OrdinalIgnoreCase) < 0)
                .ToList();
            return new MeshTopologySnapshot(_nodes.Values.ToList(), links);
        }
    }

    public MeshReachabilityResult PerformBfsReachability(string gatewayNodeId)
    {
        lock (_sync)
        {
            if (!_nodes.TryGetValue(gatewayNodeId, out var gateway) || !gateway.IsGateway)
                throw new KeyNotFoundException($"Gateway '{gatewayNodeId}' was not found.");

            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var queue = new Queue<string>();
            if (gateway.IsOnline)
            {
                queue.Enqueue(gatewayNodeId);
                visited.Add(gatewayNodeId);
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var link in _adjacencyList[current])
                {
                    if (_nodes[link.TargetNodeId].IsOnline && visited.Add(link.TargetNodeId))
                        queue.Enqueue(link.TargetNodeId);
                }
            }

            var isolated = _nodes.Values.Where(node => node.IsOnline && !visited.Contains(node.NodeId)).Select(node => node.NodeId).ToList();
            var offline = _nodes.Values.Where(node => !node.IsOnline).Select(node => node.NodeId).ToList();
            return new MeshReachabilityResult(gatewayNodeId, visited.ToList(), isolated, offline, DateTime.UtcNow);
        }
    }

    public MeshBackboneResult CalculateMinimumSpanningTreePrims(string startNodeId)
    {
        lock (_sync)
        {
            if (!_nodes.TryGetValue(startNodeId, out var start) || !start.IsOnline)
                throw new KeyNotFoundException($"Online start node '{startNodeId}' was not found.");

            var selected = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { startNodeId };
            var edges = new List<MeshLink>();
            var edgeQueue = new PriorityQueue<MeshLink, double>();
            PushOnlineEdges(startNodeId, selected, edgeQueue);

            while (edgeQueue.TryDequeue(out var link, out _))
            {
                if (!selected.Add(link.TargetNodeId))
                    continue;
                edges.Add(link);
                PushOnlineEdges(link.TargetNodeId, selected, edgeQueue);
            }

            var unconnected = _nodes.Values.Where(node => node.IsOnline && !selected.Contains(node.NodeId)).Select(node => node.NodeId).ToList();
            return new MeshBackboneResult(startNodeId, edges, unconnected, edges.Sum(link => link.LinkCost));
        }
    }

    public MeshRouteResult CalculateLowestEnergyRoute(string sourceNodeId, string destinationNodeId)
    {
        lock (_sync)
        {
            if (!_nodes.TryGetValue(sourceNodeId, out var source) || !source.IsOnline ||
                !_nodes.TryGetValue(destinationNodeId, out var destination) || !destination.IsOnline)
                return new MeshRouteResult(sourceNodeId, destinationNodeId, Array.Empty<MeshLink>(), 0, false);

            var distances = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { [sourceNodeId] = 0 };
            var previous = new Dictionary<string, MeshLink>(StringComparer.OrdinalIgnoreCase);
            var queue = new PriorityQueue<string, double>();
            queue.Enqueue(sourceNodeId, 0);

            while (queue.TryDequeue(out var current, out var queuedDistance))
            {
                if (queuedDistance > distances[current])
                    continue;
                if (string.Equals(current, destinationNodeId, StringComparison.OrdinalIgnoreCase))
                    break;

                foreach (var link in _adjacencyList[current])
                {
                    if (!_nodes[link.TargetNodeId].IsOnline)
                        continue;
                    var candidate = queuedDistance + link.LinkCost;
                    if (distances.TryGetValue(link.TargetNodeId, out var knownDistance) && candidate >= knownDistance)
                        continue;
                    distances[link.TargetNodeId] = candidate;
                    previous[link.TargetNodeId] = link;
                    queue.Enqueue(link.TargetNodeId, candidate);
                }
            }

            if (!distances.TryGetValue(destinationNodeId, out var totalCost))
                return new MeshRouteResult(sourceNodeId, destinationNodeId, Array.Empty<MeshLink>(), 0, false);

            var hops = new List<MeshLink>();
            var nodeId = destinationNodeId;
            while (!string.Equals(nodeId, sourceNodeId, StringComparison.OrdinalIgnoreCase))
            {
                var link = previous[nodeId];
                hops.Add(link);
                nodeId = link.SourceNodeId;
            }
            hops.Reverse();
            return new MeshRouteResult(sourceNodeId, destinationNodeId, hops, totalCost, true);
        }
    }

    private void PushOnlineEdges(string nodeId, HashSet<string> selected, PriorityQueue<MeshLink, double> queue)
    {
        foreach (var link in _adjacencyList[nodeId])
        {
            if (_nodes[link.TargetNodeId].IsOnline && !selected.Contains(link.TargetNodeId))
                queue.Enqueue(link, link.LinkCost);
        }
    }

    private void AddDemoLink(string source, string target, double joulesPerPacket) =>
        AddLink(new MeshLink(source, target, joulesPerPacket, -55 - joulesPerPacket * 10, 0.01 * joulesPerPacket));
}
