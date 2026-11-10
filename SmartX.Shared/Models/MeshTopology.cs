namespace SmartX.Shared.Models;

public sealed record MeshNode(
    string NodeId,
    string TruckId,
    string SensorName,
    double Latitude,
    double Longitude,
    bool IsGateway,
    bool IsOnline,
    int BatteryLevelPercent);

public sealed record MeshLink(
    string SourceNodeId,
    string TargetNodeId,
    double LinkCost,
    double RssiDbm,
    double PacketLossRate);

public sealed record MeshTopologySnapshot(
    IReadOnlyList<MeshNode> Nodes,
    IReadOnlyList<MeshLink> Links);

public sealed record MeshReachabilityResult(
    string GatewayId,
    IReadOnlyList<string> ReachableNodeIds,
    IReadOnlyList<string> IsolatedOnlineNodeIds,
    IReadOnlyList<string> OfflineNodeIds,
    DateTime ScannedAtUtc);

public sealed record MeshRouteResult(
    string SourceNodeId,
    string DestinationNodeId,
    IReadOnlyList<MeshLink> Hops,
    double TotalEnergyJoules,
    bool IsReachable);

public sealed record MeshBackboneResult(
    string StartNodeId,
    IReadOnlyList<MeshLink> Links,
    IReadOnlyList<string> UnconnectedOnlineNodeIds,
    double TotalEnergyJoules);

public sealed record SensorTelemetryReading(
    string SensorMac,
    DateTime TimestampUtc,
    TruckTelemetry Telemetry);
