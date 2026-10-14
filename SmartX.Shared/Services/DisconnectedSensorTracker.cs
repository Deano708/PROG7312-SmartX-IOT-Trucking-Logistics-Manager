using System.Collections.Generic;
using System.Linq;

namespace SmartX.Shared.Services;

public sealed class DisconnectedSensorTracker
{
    // HashSet holding unique keys for active disconnected sensor nodes (e.g., "TRK-001:24:6F:28:AB:C1:01")
    private readonly HashSet<string> _disconnectedSensorKeys = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _sync = new();


    /// Attempts to register a disconnected sensor error state.
    /// Returns true if this is a newly detected disconnection (allowing the alert to be raised).
    /// Returns false if the sensor is already recorded as disconnected, preventing duplicate alert spam.

    public bool RegisterDisconnection(string truckId, string deviceMac)
    {
        var uniqueKey = $"{truckId}:{deviceMac}";
        lock (_sync)
        {
            // HashSet.Add returns true only if the element was successfully added (was not already present)
            return _disconnectedSensorKeys.Add(uniqueKey);
        }
    }

 
    /// Clears the disconnection state when telemetry resumes and the sensor reconnects.
    public bool ResolveDisconnection(string truckId, string deviceMac)
    {
        var uniqueKey = $"{truckId}:{deviceMac}";
        lock (_sync)
        {
            return _disconnectedSensorKeys.Remove(uniqueKey);
        }
    }

    /// Checks if a specific sensor node is currently logged as disconnected.
    public bool IsDisconnected(string truckId, string deviceMac)
    {
        var uniqueKey = $"{truckId}:{deviceMac}";
        lock (_sync)
        {
            return _disconnectedSensorKeys.Contains(uniqueKey);
        }
    }

    /// Returns a snapshot of all currently active unique disconnected node keys.
    public IReadOnlyCollection<string> GetActiveDisconnectedNodes()
    {
        lock (_sync)
        {
            return _disconnectedSensorKeys.ToArray();
        }
    }
}