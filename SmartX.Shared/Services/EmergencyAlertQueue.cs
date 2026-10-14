using System.Globalization;
using SmartX.Shared.Models;

namespace SmartX.Shared.Services;

public sealed class EmergencyAlertQueue
{
    private const float LowFuelThresholdLiters = 100f;
    private readonly object _sync = new();
    private readonly Dictionary<string, QueueEntry> _activeAlerts = new(StringComparer.OrdinalIgnoreCase);
    private readonly DisconnectedSensorTracker _disconnectedTracker = new();
    private PriorityQueue<QueueEntry, (int Severity, int TruckNumber, string TruckId, int AlertType)> _queue = new();
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
                         .Where(entry => entry.Alert.TruckId == telemetry.TruckId &&
                                         entry.Alert.Type != FleetAlertType.VehicleAlert &&
                                         !detectedTypes.Contains(entry.Alert.Type))
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

    private static (int Severity, int TruckNumber, string TruckId, int AlertType) GetPriority(FleetAlert alert) =>
        ((int)alert.Severity, GetTruckNumber(alert.TruckId), alert.TruckId, (int)alert.Type);

    private static int GetTruckNumber(string truckId)
    {
        var digitStart = truckId.Length;
        while (digitStart > 0 && char.IsAsciiDigit(truckId[digitStart - 1]))
        {
            digitStart--;
        }

        return digitStart < truckId.Length &&
               int.TryParse(truckId.AsSpan(digitStart), NumberStyles.None, CultureInfo.InvariantCulture, out var truckNumber)
            ? truckNumber
            : int.MaxValue;
    }

    private void RebuildQueue()
    {
        _queue = new PriorityQueue<QueueEntry, (int Severity, int TruckNumber, string TruckId, int AlertType)>();
        foreach (var entry in _activeAlerts.Values)
        {
            _queue.Enqueue(entry, GetPriority(entry.Alert));
        }
    }

    public void ProcessSensorHeartbeat(TelemetryPacket<TruckTelemetry> packet, bool isConnected)
    {
        var truckId = packet.Payload.TruckId;
        var mac = packet.DeviceMac;
        var alertId = $"{truckId}:{mac}:Disconnected";

        lock (_sync)
        {
            if (!isConnected)
            {
                if (_disconnectedTracker.RegisterDisconnection(truckId, mac))
                {
                    var raisedAt = DateTime.UtcNow;
                    EnqueueAlert(new FleetAlert
                    {
                        Id = alertId,
                        TruckId = truckId,
                        DriverName = packet.Payload.DriverName,
                        Type = FleetAlertType.VehicleAlert,
                        Severity = FleetAlertSeverity.Critical,
                        Message = $"Hardware sensor node [{mac}] disconnected from truck telemetry bus.",
                        FirstRaisedAt = raisedAt,
                        LastUpdatedAt = raisedAt
                    });
                }
            }
            else
            {
                _disconnectedTracker.ResolveDisconnection(truckId, mac);
                _activeAlerts.Remove(alertId);

                if (_queue.Count > Math.Max(128, _activeAlerts.Count * 4))
                {
                    RebuildQueue();
                }
            }
        }
    }

    private void EnqueueAlert(FleetAlert alert)
    {
        lock (_sync)
        {
            if (_activeAlerts.TryGetValue(alert.Id, out var existing))
            {
                alert.FirstRaisedAt = existing.Alert.FirstRaisedAt;
            }

            var entry = new QueueEntry(alert, ++_version);
            _activeAlerts[alert.Id] = entry;
            _queue.Enqueue(entry, GetPriority(alert));

            if (_queue.Count > Math.Max(128, _activeAlerts.Count * 4))
            {
                RebuildQueue();
            }
        }
    }


    private sealed record QueueEntry(FleetAlert Alert, long Version);
    private sealed record DetectedAlert(FleetAlertType Type, FleetAlertSeverity Severity, string Message);
}
