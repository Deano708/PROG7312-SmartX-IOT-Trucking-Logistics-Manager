namespace SmartX.Shared.Models;

// Generic zero-boxing wrapper for incoming ESP32 payloads
public class TelemetryPacket<T> where T : struct
{
    public string DeviceMac { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public T Payload { get; set; }

    public TelemetryPacket(string deviceMac, T payload)
    {
        DeviceMac = deviceMac;
        Payload = payload;
    }
}

// Concrete telemetry payload for KZN fleet tracking
public struct TruckTelemetry
{
    public string TruckId { get; set; }
    public string DriverName { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string NearestTown { get; set; }
    public float SpeedKmh { get; set; }
    public double DistanceTraveledKm { get; set; }
    public float FuelLevelLiters { get; set; }

    // Operator Overloading for direct delta/aggregation calculations
    public static TruckTelemetry operator +(TruckTelemetry a, TruckTelemetry b) => new()
    {
        TruckId = $"{a.TruckId}&{b.TruckId}",
        DistanceTraveledKm = a.DistanceTraveledKm + b.DistanceTraveledKm,
        FuelLevelLiters = a.FuelLevelLiters + b.FuelLevelLiters
    };
}

// Recursive validation model for nested sensor deployment topologies
public class DeploymentNode
{
    public string NodeId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Environmental, Power, Actuator
    public bool IsSecurelyConfigured { get; set; }
    public List<DeploymentNode> SubNodes { get; set; } = new();

    public bool ValidateTopology()
    {
        if (!IsSecurelyConfigured) return false;
        foreach (var subNode in SubNodes)
        {
            if (!subNode.ValidateTopology()) return false;
        }
        return true;
    }
}
