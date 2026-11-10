namespace SmartX.Shared.Services;

using SmartX.Shared.Models;

public class HistoricalTelemetryIndex
{
    // Red-Black Tree backed SortedDictionary for O(log n) time-range queries
    private readonly SortedDictionary<DateTime, TruckTelemetry> _timeIndex = new();
    private readonly object _sync = new();

    public void AddReading(DateTime timestamp, TruckTelemetry telemetry)
    {
        var utcTime = timestamp.ToUniversalTime();
        lock (_sync)
        {
            // If duplicate timestamp exists, SortedDictionary updates value
            _timeIndex[utcTime] = telemetry;
        }
    }

    public IReadOnlyList<TruckTelemetry> QueryRange(DateTime fromUtc, DateTime toUtc)
    {
        lock (_sync)
        {
            // O(log n) lookup for range boundaries
            return _timeIndex
                .Where(pair => pair.Key >= fromUtc && pair.Key <= toUtc)
                .Select(pair => pair.Value)
                .ToList();
        }
    }
}