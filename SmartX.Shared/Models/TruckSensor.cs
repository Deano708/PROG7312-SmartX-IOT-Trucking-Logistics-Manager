namespace SmartX.Shared.Models;

public enum TruckSensorCategory
{
    FuelAndFluids,
    GpsAndTelemetry,
    StructuralAndSafety
}

public class TruckSensorProfile
{
    public string SensorId { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string DeviceMac { get; set; } = string.Empty;
    public string AssignedTruckId { get; set; } = string.Empty;
    public TruckSensorCategory Category { get; set; }
    public string AssignedSensorName { get; set; } = string.Empty;
    public string SpecSheetFileName { get; set; } = string.Empty;
    public byte[]? FileData { get; set; }
    public string? FileContentType { get; set; }
    public bool IsActive { get; set; } = true;
}