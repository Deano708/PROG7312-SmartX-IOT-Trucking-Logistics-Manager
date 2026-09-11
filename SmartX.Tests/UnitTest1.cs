using Xunit;
using Assert = Xunit.Assert;
using SmartX.Shared.Models;
using SmartX.Shared.Services;

namespace SmartX.Tests;

public class SmartXEngineTests
{
    [Fact]
    public void Test_JaggedArrayFlattener_FlattensWithoutLoss()
    {
        var processor = new TelemetryBatchProcessor();
        int[][] rawBatches = new int[][]
        {
            new int[] { 100, 105 },
            new int[] { 110, 115, 120 }
        };
        string[] macs = new[] { "MAC1", "MAC2" };

        var result = processor.ProcessJaggedBatch(rawBatches, macs);

        Assert.Equal(5, result.Count);
        Assert.Equal(100, result[0].Payload);
        Assert.Equal("MAC1", result[0].DeviceMac);
    }

    [Fact]
    public void Test_RecursiveTopologyValidation_DetectsMisconfiguration()
    {
        var root = new DeploymentNode
        {
            NodeId = "Facility A",
            IsSecurelyConfigured = true,
            SubNodes = new()
            {
                new DeploymentNode
                {
                    NodeId = "Zone 1",
                    IsSecurelyConfigured = true,
                    SubNodes = new()
                    {
                        new DeploymentNode { NodeId = "Sub-Zone B", IsSecurelyConfigured = false } // Flawed node
                    }
                }
            }
        };

        Assert.False(root.ValidateTopology());
    }

    [Fact]
    public void Test_OperatorOverloading_AggregatesTruckTelemetry()
    {
        var truck1 = new TruckTelemetry { TruckId = "TRK-01", DistanceTraveledKm = 150.0, FuelLevelLiters = 40.0f };
        var truck2 = new TruckTelemetry { TruckId = "TRK-02", DistanceTraveledKm = 200.0, FuelLevelLiters = 50.0f };

        var combined = truck1 + truck2;

        Assert.Equal(350.0, combined.DistanceTraveledKm);
        Assert.Equal(90.0f, combined.FuelLevelLiters);
    }
}