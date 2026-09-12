namespace SmartX.Shared.Models;

public enum TruckSensorCategory
{
    TelemetryAndKinetics,
    FuelAndFluids,
    ColdChainAndCargo,
    SecurityAndActuators,
    StructuralAndSafety
}

public enum TruckDeploymentZone
{
    CabDashboard,
    EngineBay,
    FuelTankPrimary,
    FuelTankSecondary,
    RefrigeratedTrailerBox,
    TrailerTailgate,
    WheelAxleAssembly
}

public class TruckSensorProfile
{
    public string SensorId { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string DeviceMac { get; set; } = string.Empty;
    public string AssignedTruckId { get; set; } = string.Empty;
    public TruckSensorCategory Category { get; set; }
    public TruckDeploymentZone Zone { get; set; }
    public string SpecSheetFileName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}