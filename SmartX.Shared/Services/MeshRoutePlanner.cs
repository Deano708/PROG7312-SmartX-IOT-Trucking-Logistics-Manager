namespace SmartX.Shared.Services;

using SmartX.Shared.Models;

public class MeshRoutePlanner
{
    private readonly Dictionary<string, MeshNode> _nodes = new();
    private readonly Dictionary<string, List<MeshLink>> _adjacencyList = new();
    private readonly object _sync = new();

    public void RegisterNode(MeshNode node)
    {
        lock (_sync)
        {
            _nodes[node.NodeId] = node;
            if (!_adjacencyList.ContainsKey(node.NodeId))
            {
                _adjacencyList[node.NodeId] = new();
            }
        }
    }

    public void AddLink(MeshLink link)
    {
        lock (_sync)
        {
            if (!_adjacencyList.ContainsKey(link.SourceNodeId))
            {
                _adjacencyList[link.SourceNodeId] = new();
            }
            _adjacencyList[link.SourceNodeId].Add(link);

            // Ensure undirected representation for MST algorithms if links are bidirectional
            if (!_adjacencyList.ContainsKey(link.TargetNodeId))
            {
                _adjacencyList[link.TargetNodeId] = new();
            }
            _adjacencyList[link.TargetNodeId].Add(new MeshLink(
                link.TargetNodeId,
                link.SourceNodeId,
                link.LinkCost,
                link.RssiDbm,
                link.PacketLossRate
            ));
        }
    }

    /// <summary>
    /// BFS Traversal to identify all connected nodes, subnets, and isolated/offline devices.
    /// </summary>
    public (List<string> ReachableNodes, List<string> IsolatedNodes) PerformBfsReachability(string gatewayNodeId)
    {
        lock (_sync)
        {
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var queue = new Queue<string>();

            if (_nodes.ContainsKey(gatewayNodeId))
            {
                queue.Enqueue(gatewayNodeId);
                visited.Add(gatewayNodeId);
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (_adjacencyList.TryGetValue(current, out var links))
                {
                    foreach (var link in links)
                    {
                        if (_nodes.TryGetValue(link.TargetNodeId, out var targetNode) && targetNode.IsOnline && !visited.Contains(link.TargetNodeId))
                        {
                            visited.Add(link.TargetNodeId);
                            queue.Enqueue(link.TargetNodeId);
                        }
                    }
                }
            }

            var allOnlineNodeIds = _nodes.Values.Where(n => n.IsOnline).Select(n => n.NodeId).ToList();
            var isolated = allOnlineNodeIds.Where(id => !visited.Contains(id)).ToList();

            return (visited.ToList(), isolated);
        }
    }

    /// <summary>
    /// Prim's Algorithm: Calculates the Minimum Spanning Tree (MST) 
    /// to establish the most power-efficient routing backbone across all mesh nodes.
    /// </summary>
    public List<MeshLink> CalculateMinimumSpanningTreePrims(string startNodeId)
    {
        lock (_sync)
        {
            var mstEdges = new List<MeshLink>();
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Priority queue stores edges to evaluate: (Source, Target, Cost)
            var edgeQueue = new PriorityQueue<(string Source, string Target, MeshLink Link), double>();

            if (!_nodes.ContainsKey(startNodeId)) return mstEdges;

            visited.Add(startNodeId);
            PushAdjacentEdges(startNodeId, visited, edgeQueue);

            while (edgeQueue.Count > 0)
            {
                if (!edgeQueue.TryDequeue(out var edge, out var cost))
                    break;

                if (visited.Contains(edge.Target))
                    continue;

                visited.Add(edge.Target);
                mstEdges.Add(edge.Link);

                PushAdjacentEdges(edge.Target, visited, edgeQueue);
            }

            return mstEdges;
        }
    }

    private void PushAdjacentEdges(string nodeId, HashSet<string> visited, PriorityQueue<(string Source, string Target, MeshLink Link), double> edgeQueue)
    {
        if (_adjacencyList.TryGetValue(nodeId, out var links))
        {
            foreach (var link in links)
            {
                if (_nodes.TryGetValue(link.TargetNodeId, out var targetNode) && targetNode.IsOnline && !visited.Contains(link.TargetNodeId))
                {
                    edgeQueue.Enqueue((link.SourceNodeId, link.TargetNodeId, link), link.LinkCost);
                }
            }
        }
    }
}