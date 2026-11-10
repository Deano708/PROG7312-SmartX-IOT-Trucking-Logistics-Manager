namespace SmartX.Shared.Models;

public record MeshNode(
    string NodeId,
    string TruckId,
    string SensorName,
    double Latitude,
    double Longitude,
    bool IsGateway,
    bool IsOnline,
    int BatteryLevelPercent
);

public record MeshLink(
    string SourceNodeId,
    string TargetNodeId,
    double LinkCost,       // Composite metric: Energy (J/packet) + Latency (ms) + Packet Loss penalty
    double RssiDbm,        // Signal strength (e.g., -68 dBm)
    double PacketLossRate  // Percentage (0.0 to 1.0)
);