namespace SmartX.Shared.Models;

using Microsoft.ML.Data;

public class FuelInputData
{
    [LoadColumn(0)] public float SpeedKmh { get; set; }
    [LoadColumn(1)] public float DistanceTraveledKm { get; set; }
    [LoadColumn(2)] public float CurrentFuelLiters { get; set; }
    [LoadColumn(3)] public float PayloadWeightTons { get; set; }
    [LoadColumn(4)] public float RemainingRangeKm { get; set; }
}

public class FuelPrediction
{
    [ColumnName("Score")]
    public float PredictedRemainingRangeKm { get; set; }
}