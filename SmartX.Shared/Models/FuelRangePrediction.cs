namespace SmartX.Shared.Models;

public sealed class FuelRangePrediction
{
    public string TruckId { get; set; } = string.Empty;
    public float PredictedRemainingRangeKm { get; set; }
    public DateTime PredictedAtUtc { get; set; }
}
