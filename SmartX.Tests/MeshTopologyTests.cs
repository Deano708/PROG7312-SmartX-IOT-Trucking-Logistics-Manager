using SmartX.Shared.Models;
using SmartX.Shared.Services;
using Xunit;

namespace SmartX.Tests;

public class MeshTopologyTests
{
    [Fact]
    public void BfsReportsReachableOnlineAndOfflineNodesSeparately()
    {
        var planner = new MeshRoutePlanner();

        var result = planner.PerformBfsReachability("GW-01");

        Assert.Contains("GW-01", result.ReachableNodeIds);
        Assert.Equal(12, result.ReachableNodeIds.Count);
        Assert.Empty(result.IsolatedOnlineNodeIds);
        Assert.Equal(new[] { MeshRoutePlanner.SensorMac(11) }, result.OfflineNodeIds);
    }

    [Fact]
    public void PrimsBackboneSpansEveryOnlineNodeAndReportsEnergyCost()
    {
        var planner = new MeshRoutePlanner();

        var result = planner.CalculateMinimumSpanningTreePrims("GW-01");

        Assert.Equal(11, result.Links.Count);
        Assert.Empty(result.UnconnectedOnlineNodeIds);
        Assert.Equal(result.Links.Sum(link => link.LinkCost), result.TotalEnergyJoules);
        Assert.All(result.Links, link => Assert.True(link.LinkCost > 0));
    }

    [Fact]
    public void DijkstraFindsTheLowestEnergySensorToGatewayRoute()
    {
        var planner = new MeshRoutePlanner();

        var result = planner.CalculateLowestEnergyRoute(MeshRoutePlanner.SensorMac(3), "GW-01");

        Assert.True(result.IsReachable);
        Assert.Equal(2.0, result.TotalEnergyJoules, 6);
        Assert.Equal(new[] { MeshRoutePlanner.SensorMac(2), "GW-01" }, result.Hops.Select(hop => hop.TargetNodeId));
    }

    [Fact]
    public void AvlHistoryIndexUpsertsDuplicateSensorTimestampsAndReturnsChronologicalRange()
    {
        var index = new HistoricalTelemetryIndex();
        var start = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        index.AddReading("sensor-a", start.AddHours(3), new TruckTelemetry { TruckId = "TRK-001" });
        index.AddReading("sensor-a", start, new TruckTelemetry { TruckId = "TRK-001" });
        index.AddReading("sensor-a", start.AddHours(2), new TruckTelemetry { TruckId = "TRK-001", SpeedKmh = 20 });
        index.AddReading("sensor-a", start.AddHours(2), new TruckTelemetry { TruckId = "TRK-001", SpeedKmh = 30 });
        index.AddReading("sensor-b", start.AddHours(2), new TruckTelemetry { TruckId = "TRK-002" });

        var results = index.QueryRange("SENSOR-A", start, start.AddHours(2));

        Assert.Equal(2, results.Count);
        Assert.All(results, reading => Assert.Equal("sensor-a", reading.SensorMac));
        Assert.Equal(new[] { start, start.AddHours(2) }, results.Select(reading => reading.TimestampUtc));
        Assert.Equal(new[] { 0, 30 }, results.Select(reading => (int)reading.Telemetry.SpeedKmh));
    }

    [Theory]
    [InlineData(2026, 11, 31)]
    [InlineData(2026, 3, 28)]
    public void PreviousMonthSamplesCoverEveryDayForEveryTruck(int year, int month, int expectedDays)
    {
        var fleet = Enumerable.Range(1, 12)
            .Select(number => new TruckTelemetry { TruckId = $"TRK-{number:000}" })
            .ToArray();
        var samples = TruckRouteSimulator.GeneratePreviousMonthSamples(
            fleet,
            new DateTime(year, month, 12, 12, 0, 0, DateTimeKind.Utc));

        Assert.Equal(12 * expectedDays, samples.Count);
        Assert.Equal(12, samples.Select(sample => sample.Payload.TruckId).Distinct().Count());
        Assert.All(samples, sample => Assert.Equal("02:42:AC:11:00:" + sample.Payload.TruckId[^2..], sample.DeviceMac));
        var currentMonthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var previousMonthStart = currentMonthStart.AddMonths(-1);
        Assert.All(samples, sample => Assert.InRange(sample.Timestamp, previousMonthStart, currentMonthStart.AddTicks(-1)));
        Assert.Equal(expectedDays, samples.Select(sample => sample.Timestamp.Date).Distinct().Count());
    }
}
