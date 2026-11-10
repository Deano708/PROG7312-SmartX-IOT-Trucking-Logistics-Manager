namespace SmartX.Shared.Services;

using SmartX.Shared.Models;

public class HistoricalTelemetryIndex
{
    private readonly Dictionary<string, AvlNode> _sensorRoots = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _sync = new();

    public void AddReading(string sensorMac, DateTime timestamp, TruckTelemetry telemetry)
    {
        if (string.IsNullOrWhiteSpace(sensorMac))
            throw new ArgumentException("A sensor MAC address is required.", nameof(sensorMac));
        ArgumentNullException.ThrowIfNull(telemetry);
        var utcTime = timestamp.ToUniversalTime();

        lock (_sync)
        {
            _sensorRoots.TryGetValue(sensorMac, out var root);
            _sensorRoots[sensorMac] = Insert(root, new SensorTelemetryReading(sensorMac, utcTime, telemetry));
        }
    }

    public IReadOnlyList<SensorTelemetryReading> QueryRange(string sensorMac, DateTime fromUtc, DateTime toUtc)
    {
        if (string.IsNullOrWhiteSpace(sensorMac))
            throw new ArgumentException("A sensor MAC address is required.", nameof(sensorMac));
        if (fromUtc > toUtc)
            throw new ArgumentException("The range start must not be later than the range end.");

        var from = fromUtc.ToUniversalTime();
        var to = toUtc.ToUniversalTime();
        lock (_sync)
        {
            var results = new List<SensorTelemetryReading>();
            if (_sensorRoots.TryGetValue(sensorMac, out var root))
                CollectRange(root, sensorMac, from, to, results);
            return results;
        }
    }

    private static AvlNode Insert(AvlNode? node, SensorTelemetryReading reading)
    {
        if (node is null)
            return new AvlNode(reading.TimestampUtc, reading);

        if (reading.TimestampUtc < node.TimestampUtc)
            node.Left = Insert(node.Left, reading);
        else if (reading.TimestampUtc > node.TimestampUtc)
            node.Right = Insert(node.Right, reading);
        else
            node.Readings.Add(reading);

        UpdateHeight(node);
        var balance = Height(node.Left) - Height(node.Right);
        if (balance > 1)
        {
            if (reading.TimestampUtc > node.Left!.TimestampUtc)
                node.Left = RotateLeft(node.Left);
            return RotateRight(node);
        }
        if (balance < -1)
        {
            if (reading.TimestampUtc < node.Right!.TimestampUtc)
                node.Right = RotateRight(node.Right);
            return RotateLeft(node);
        }
        return node;
    }

    private static void CollectRange(AvlNode? node, string sensorMac, DateTime from, DateTime to, List<SensorTelemetryReading> results)
    {
        if (node is null)
            return;
        if (from < node.TimestampUtc)
            CollectRange(node.Left, sensorMac, from, to, results);
        if (node.TimestampUtc >= from && node.TimestampUtc <= to)
            results.AddRange(node.Readings);
        if (to > node.TimestampUtc)
            CollectRange(node.Right, sensorMac, from, to, results);
    }

    private static int Height(AvlNode? node) => node?.Height ?? 0;

    private static void UpdateHeight(AvlNode node) => node.Height = Math.Max(Height(node.Left), Height(node.Right)) + 1;

    private static AvlNode RotateRight(AvlNode root)
    {
        var pivot = root.Left!;
        root.Left = pivot.Right;
        pivot.Right = root;
        UpdateHeight(root);
        UpdateHeight(pivot);
        return pivot;
    }

    private static AvlNode RotateLeft(AvlNode root)
    {
        var pivot = root.Right!;
        root.Right = pivot.Left;
        pivot.Left = root;
        UpdateHeight(root);
        UpdateHeight(pivot);
        return pivot;
    }

    private sealed class AvlNode(DateTime timestampUtc, SensorTelemetryReading reading)
    {
        public DateTime TimestampUtc { get; } = timestampUtc;
        public List<SensorTelemetryReading> Readings { get; } = [reading];
        public AvlNode? Left { get; set; }
        public AvlNode? Right { get; set; }
        public int Height { get; set; } = 1;
    }
}
