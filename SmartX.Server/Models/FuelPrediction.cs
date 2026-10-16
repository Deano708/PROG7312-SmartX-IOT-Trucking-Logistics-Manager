namespace SmartX.Server.Models;

using Microsoft.ML.Data;

/*
   Title: Revolutionize Your Coding: C# Machine Learning Essentials with ML.NET!
   Author: Nick Proud
   Date: 2 Years Ago
   Date accessed: 14/10/2026
   Availability: https://www.youtube.com/watch?v=R3kRf7hNVMg
*/

/*
   Title: Train and evaluate a model
   Author: Microsoft Learn
   Date: N/A
   Date accessed: 14/10/2026
   Availability: https://learn.microsoft.com/en-us/dotnet/machine-learning/how-to-guides/train-machine-learning-model-ml-net
*/


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