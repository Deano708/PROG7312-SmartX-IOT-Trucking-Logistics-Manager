using System.Text;
using SmartX.Shared.Models;

namespace SmartX.Client.Services;

public class TruckSensorStateService
{
    public List<TruckTelemetry> Fleet { get; private set; } = new()
    {
        new() { TruckId = "TRK-001", DriverName = "Sipho Zulu", Latitude = -29.8587, Longitude = 31.0218, NearestTown = "Durban Port (N3)", SpeedKmh = 82.5f, DistanceTraveledKm = 420.0, FuelLevelLiters = 310.0f },
        new() { TruckId = "TRK-002", DriverName = "Johan Botha", Latitude = -26.2041, Longitude = 28.0473, NearestTown = "City Deep JHB (N1)", SpeedKmh = 0.0f, DistanceTraveledKm = 1150.0, FuelLevelLiters = 450.0f },
        new() { TruckId = "TRK-003", DriverName = "Thabo Mokoena", Latitude = -33.9249, Longitude = 18.4241, NearestTown = "Cape Town Harbour (N1)", SpeedKmh = 65.0f, DistanceTraveledKm = 1420.0, FuelLevelLiters = 280.0f },
        new() { TruckId = "TRK-004", DriverName = "Pieter van Zyl", Latitude = -33.9608, Longitude = 25.6022, NearestTown = "Gqeberha / PE (N2)", SpeedKmh = 78.0f, DistanceTraveledKm = 890.0, FuelLevelLiters = 340.0f },
        new() { TruckId = "TRK-005", DriverName = "Bongani Dlamini", Latitude = -23.9045, Longitude = 29.4688, NearestTown = "Polokwane (N1 North)", SpeedKmh = 88.0f, DistanceTraveledKm = 620.0, FuelLevelLiters = 210.0f },
        new() { TruckId = "TRK-006", DriverName = "Willem Nel", Latitude = -29.1181, Longitude = 26.2243, NearestTown = "Bloemfontein (N1 Central)", SpeedKmh = 80.0f, DistanceTraveledKm = 780.0, FuelLevelLiters = 390.0f },
        new() { TruckId = "TRK-007", DriverName = "Kabelo Molefe", Latitude = -25.6544, Longitude = 27.2559, NearestTown = "Rustenburg (N4 Platinum)", SpeedKmh = 72.0f, DistanceTraveledKm = 310.0, FuelLevelLiters = 180.0f },
        new() { TruckId = "TRK-008", DriverName = "Andre Joubert", Latitude = -33.0153, Longitude = 27.8935, NearestTown = "East London (N2)", SpeedKmh = 55.0f, DistanceTraveledKm = 950.0, FuelLevelLiters = 260.0f },
        new() { TruckId = "TRK-009", DriverName = "Lungelo Nxumalo", Latitude = -25.4753, Longitude = 30.9694, NearestTown = "Mbombela (N4 East)", SpeedKmh = 84.0f, DistanceTraveledKm = 510.0, FuelLevelLiters = 300.0f },
        new() { TruckId = "TRK-010", DriverName = "Gerhard Coetzee", Latitude = -28.7282, Longitude = 24.7499, NearestTown = "Kimberley (N12)", SpeedKmh = 79.0f, DistanceTraveledKm = 670.0, FuelLevelLiters = 230.0f },
        new() { TruckId = "TRK-011", DriverName = "Tendai Moyo", Latitude = -28.7807, Longitude = 32.0383, NearestTown = "Richards Bay Port (R34)", SpeedKmh = 60.0f, DistanceTraveledKm = 290.0, FuelLevelLiters = 190.0f },
        new() { TruckId = "TRK-012", DriverName = "Jaco Venter", Latitude = -28.4478, Longitude = 21.2561, NearestTown = "Upington (N14 Corridor)", SpeedKmh = 90.0f, DistanceTraveledKm = 1100.0, FuelLevelLiters = 410.0f }
    };

    public List<TruckSensorProfile> Sensors { get; private set; } = new();

    public TruckSensorStateService()
    {
        var sampleDocBytes = Encoding.UTF8.GetBytes("{\"calibration\": \"verified\", \"voltage\": \"3.3V\", \"status\": \"OK\"}");

        foreach (var truck in Fleet)
        {
            Sensors.Add(new TruckSensorProfile
            {
                AssignedTruckId = truck.TruckId,
                DeviceMac = $"02:42:AC:11:00:{truck.TruckId[^2..]}",
                Category = TruckSensorCategory.GpsAndTelemetry,
                AssignedSensorName = "GPS Transponder",
                SpecSheetFileName = "obd_gps.json",
                FileData = sampleDocBytes,
                FileContentType = "application/json"
            });

            Sensors.Add(new TruckSensorProfile
            {
                AssignedTruckId = truck.TruckId,
                DeviceMac = $"24:6F:28:AB:C1:{truck.TruckId[^2..]}",
                Category = TruckSensorCategory.FuelAndFluids,
                AssignedSensorName = "(FLS) Fuel Level Sensor",
                SpecSheetFileName = "fuel_cap.json",
                FileData = sampleDocBytes,
                FileContentType = "application/json"
            });
        }
    }

    public void AddSensor(TruckSensorProfile sensor) => Sensors.Add(sensor);
    public void RemoveSensor(TruckSensorProfile sensor) => Sensors.Remove(sensor);
}