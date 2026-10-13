using System.Collections.Concurrent;
using SmartX.Shared.Models;

namespace SmartX.Server.Services;

public sealed class TelemetryHistoryStore
{
    private const int MaximumReadingsPerTruck = 48;
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<DateTime, TelemetryPacket<TruckTelemetry>>> _history = new();

    public void Ingest(TelemetryPacket<TruckTelemetry> packet)
    {
        var timestamp = packet.Timestamp.ToUniversalTime();
        packet.Timestamp = timestamp;
        var truckHistory = _history.GetOrAdd(packet.Payload.TruckId, _ => new ConcurrentDictionary<DateTime, TelemetryPacket<TruckTelemetry>>());
        truckHistory[timestamp] = packet;

        foreach (var oldTimestamp in truckHistory.Keys.Order().Take(Math.Max(0, truckHistory.Count - MaximumReadingsPerTruck)))
        {
            truckHistory.TryRemove(oldTimestamp, out _);
        }
    }

    public IReadOnlyList<TelemetryPacket<TruckTelemetry>> GetHistory(string truckId) =>
        _history.TryGetValue(truckId, out var truckHistory)
            ? truckHistory.Values.OrderBy(packet => packet.Timestamp).ToArray()
            : Array.Empty<TelemetryPacket<TruckTelemetry>>();
}
