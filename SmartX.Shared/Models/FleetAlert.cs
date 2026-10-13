namespace SmartX.Shared.Models;

public enum FleetAlertType
{
    FuelLow,
    OilLevelLow,
    TyrePressureLow
}

public enum FleetAlertSeverity
{
    Critical,
    High
}

public sealed class FleetAlert
{
    public string Id { get; set; } = string.Empty;
    public string TruckId { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public FleetAlertType Type { get; set; }
    public FleetAlertSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime FirstRaisedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}
