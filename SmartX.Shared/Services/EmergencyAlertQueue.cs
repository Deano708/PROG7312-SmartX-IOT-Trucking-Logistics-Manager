using System.Globalization;
using SmartX.Shared.Models;

namespace SmartX.Shared.Services;

public sealed class EmergencyAlertQueue
{
    private const float LowFuelThresholdLiters = 100f;
    private readonly object _sync = new();
    private readonly Dictionary<string, QueueEntry> _activeAlerts = new(StringComparer.OrdinalIgnoreCase);
    private PriorityQueue<QueueEntry, (int Severity, long Recency)> _queue = new();
    private long _version;

    public void ProcessTelemetry(TelemetryPacket<TruckTelemetry> packet)
    {
        var telemetry = packet.Payload;
        var raisedAt = packet.Timestamp.Kind == DateTimeKind.Utc
            ? packet.Timestamp
            : packet.Timestamp.ToUniversalTime();
        var detectedAlerts = DetectAlerts(telemetry).ToArray();
        var detectedTypes = detectedAlerts.Select(alert => alert.Type).ToHashSet();

        lock (_sync)
        {
            foreach (var activeAlert in _activeAlerts.Values
                         .Where(entry => entry.Alert.TruckId == telemetry.TruckId && !detectedTypes.Contains(entry.Alert.Type))
                         .ToArray())
            {
                _activeAlerts.Remove(activeAlert.Alert.Id);
            }

            foreach (var detected in detectedAlerts)
            {
                var alertId = $"{telemetry.TruckId}:{detected.Type}";
                var firstRaisedAt = _activeAlerts.TryGetValue(alertId, out var existing)
                    ? existing.Alert.FirstRaisedAt
                    : raisedAt;
                var alert = new FleetAlert
                {
                    Id = alertId,
                    TruckId = telemetry.TruckId,
                    DriverName = telemetry.DriverName,
                    Type = detected.Type,
                    Severity = detected.Severity,
                    Message = detected.Message,
                    FirstRaisedAt = firstRaisedAt,
                    LastUpdatedAt = raisedAt
                };
                var entry = new QueueEntry(alert, ++_version);
                _activeAlerts[alertId] = entry;
                _queue.Enqueue(entry, GetPriority(alert));
            }

            if (_queue.Count > Math.Max(128, _activeAlerts.Count * 4))
            {
                RebuildQueue();
            }
        }
    }

    public IReadOnlyList<FleetAlert> GetActiveAlerts()
    {
        lock (_sync)
        {
            var orderedAlerts = new List<QueueEntry>(_activeAlerts.Count);
            while (_queue.TryDequeue(out var entry, out _))
            {
                if (_activeAlerts.TryGetValue(entry.Alert.Id, out var active) && active.Version == entry.Version)
                {
                    orderedAlerts.Add(entry);
                }
            }

            foreach (var entry in orderedAlerts)
            {
                _queue.Enqueue(entry, GetPriority(entry.Alert));
            }

            return orderedAlerts.Select(entry => entry.Alert).ToArray();
        }
    }

    private static IEnumerable<DetectedAlert> DetectAlerts(TruckTelemetry telemetry)
    {
        if (telemetry.FuelLevelLiters <= LowFuelThresholdLiters)
        {
            var severity = telemetry.FuelLevelLiters <= 50f ? FleetAlertSeverity.Critical : FleetAlertSeverity.High;
            var fuelLevel = telemetry.FuelLevelLiters.ToString("F0", CultureInfo.InvariantCulture);
            yield return new DetectedAlert(
                FleetAlertType.FuelLow,
                severity,
                $"Fuel is low: {fuelLevel} L remaining.");
        }

        if (telemetry.OilLevelLow)
        {
            yield return new DetectedAlert(
                FleetAlertType.OilLevelLow,
                FleetAlertSeverity.Critical,
                "Oil level is low. Stop safely and inspect the engine oil system.");
        }

        if (telemetry.TyrePressureLow)
        {
            yield return new DetectedAlert(
                FleetAlertType.TyrePressureLow,
                FleetAlertSeverity.Critical,
                "Tyre pressure is low. Reduce speed and inspect the tyres safely.");
        }
    }

    private static (int Severity, long Recency) GetPriority(FleetAlert alert) =>
        ((int)alert.Severity, -alert.LastUpdatedAt.Ticks);

    private void RebuildQueue()
    {
        _queue = new PriorityQueue<QueueEntry, (int Severity, long Recency)>();
        foreach (var entry in _activeAlerts.Values)
        {
            _queue.Enqueue(entry, GetPriority(entry.Alert));
        }
    }

    private sealed record QueueEntry(FleetAlert Alert, long Version);
    private sealed record DetectedAlert(FleetAlertType Type, FleetAlertSeverity Severity, string Message);
}
