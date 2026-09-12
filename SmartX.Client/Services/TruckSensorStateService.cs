using SmartX.Shared.Models;

namespace SmartX.Client.Services;

public class TruckSensorStateService
{
    public List<TruckSensorProfile> Sensors { get; private set; } = new()
    {
        new() { AssignedTruckId = "TRK-001", DeviceMac = "02:42:AC:11:00:02", Category = TruckSensorCategory.TelemetryAndKinetics, Zone = TruckDeploymentZone.CabDashboard, SpecSheetFileName = "gps_spec.pdf" },
        new() { AssignedTruckId = "TRK-001", DeviceMac = "24:6F:28:AB:C1:12", Category = TruckSensorCategory.ColdChainAndCargo, Zone = TruckDeploymentZone.RefrigeratedTrailerBox, SpecSheetFileName = "dht22_calib.json" }
    };

    public void AddSensor(TruckSensorProfile sensor)
    {
        Sensors.Add(sensor);
    }

    public void RemoveSensor(TruckSensorProfile sensor)
    {
        Sensors.Remove(sensor);
    }
}
