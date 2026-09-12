using SmartX.Shared.Models;

namespace SmartX.Shared.Services;

public class KznDataSeeder
{
    public Dictionary<string, List<TelemetryPacket<TruckTelemetry>>> FleetJourneys { get; set; } = new();

    public KznDataSeeder()
    {
        SeedAll12TruckJourneys();
    }

    private void SeedAll12TruckJourneys()
    {
        var truckCorridors = new List<(string Id, string Driver, double StartLat, double StartLng, double EndLat, double EndLng, string Corridor)>
        {
            ("TRK-001", "Sipho Zulu", -29.8587, 31.0218, -26.2041, 28.0473, "Durban Port to JHB (N3)"),
            ("TRK-002", "Johan Botha", -26.2041, 28.0473, -29.1181, 26.2243, "JHB to Bloemfontein (N1)"),
            ("TRK-003", "Thabo Mokoena", -33.9249, 18.4241, -32.3547, 22.5822, "Cape Town to Beaufort West (N1)"),
            ("TRK-004", "Pieter van Zyl", -33.9608, 25.6022, -33.0153, 27.8935, "Gqeberha to East London (N2)"),
            ("TRK-005", "Bongani Dlamini", -25.7479, 28.2293, -23.9045, 29.4688, "Pretoria to Polokwane (N1)"),
            ("TRK-006", "Willem Nel", -29.1181, 26.2243, -30.5982, 25.0970, "Bloemfontein to Colesberg (N1)"),
            ("TRK-007", "Kabelo Molefe", -25.7479, 28.2293, -25.6544, 27.2559, "Pretoria to Rustenburg (N4)"),
            ("TRK-008", "Andre Joubert", -33.0153, 27.8935, -31.5892, 28.7844, "East London to Mthatha (N2)"),
            ("TRK-009", "Lungelo Nxumalo", -26.2041, 28.0473, -25.4753, 30.9694, "JHB to Mbombela (N4)"),
            ("TRK-010", "Gerhard Coetzee", -26.2041, 28.0473, -28.7282, 24.7499, "JHB to Kimberley (N12)"),
            ("TRK-011", "Tendai Moyo", -29.8587, 31.0218, -28.7807, 32.0383, "Durban to Richards Bay (N2)"),
            ("TRK-012", "Jaco Venter", -28.7282, 24.7499, -28.4478, 21.2561, "Kimberley to Upington (N14)")
        };

        var baseTime = DateTime.UtcNow.AddHours(-12);

        foreach (var truck in truckCorridors)
        {
            var history = new List<TelemetryPacket<TruckTelemetry>>();

            for (int hour = 0; hour < 12; hour++)
            {
                // Linear interpolation across coordinates for 12 hours
                double fraction = hour / 11.0;
                double lat = truck.StartLat + (truck.EndLat - truck.StartLat) * fraction;
                double lng = truck.StartLng + (truck.EndLng - truck.StartLng) * fraction;

                history.Add(new TelemetryPacket<TruckTelemetry>
                {
                    DeviceMac = $"02:42:AC:11:00:{truck.Id[^2..]}",
                    Timestamp = baseTime.AddHours(hour),
                    Payload = new TruckTelemetry
                    {
                        TruckId = truck.Id,
                        DriverName = truck.Driver,
                        Latitude = lat,
                        Longitude = lng,
                        NearestTown = $"{truck.Corridor} - Hour {hour + 1}",
                        SpeedKmh = (hour == 0 || hour == 11) ? 0.0f : 78.5f + (hour % 3) * 4.0f,
                        DistanceTraveledKm = Math.Round(fraction * 600.0, 1),
                        FuelLevelLiters = (float)Math.Round(450.0 - (fraction * 180.0), 1)
                    }
                });
            }

            FleetJourneys[truck.Id] = history;
        }
    }
}