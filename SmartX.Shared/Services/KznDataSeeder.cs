namespace SmartX.Shared.Services;

using SmartX.Shared.Models;

public class KznDataSeeder
{
    private static readonly (string Town, double Lat, double Lng)[] N3Route = new[]
    {
        ("Durban Harbor", -29.8711, 31.0229),
        ("Pietermaritzburg", -29.6006, 30.3794),
        ("Mooi River", -29.2081, 29.9975),
        ("Harrismith", -28.2728, 29.1286)
    };

    public List<TelemetryPacket<TruckTelemetry>> GenerateRouteHistory(string truckId, string driver, int hours)
    {
        var history = new List<TelemetryPacket<TruckTelemetry>>();
        var startTime = DateTime.UtcNow.AddHours(-hours);
        var rand = new Random();

        for (int h = 0; h <= hours; h++)
        {
            int routeIndex = Math.Min(h % N3Route.Length, N3Route.Length - 1);
            var waypoint = N3Route[routeIndex];

            var telemetry = new TruckTelemetry
            {
                TruckId = truckId,
                DriverName = driver,
                NearestTown = waypoint.Town,
                Latitude = waypoint.Lat + (rand.NextDouble() - 0.5) * 0.02,
                Longitude = waypoint.Lng + (rand.NextDouble() - 0.5) * 0.02,
                SpeedKmh = rand.Next(70, 95),
                DistanceTraveledKm = h * 75.5,
                FuelLevelLiters = Math.Max(10, 300 - (h * 18.2f))
            };

            var packet = new TelemetryPacket<TruckTelemetry>($"02:00:00:00:{rand.Next(10, 99)}:FF", telemetry)
            {
                Timestamp = startTime.AddHours(h)
            };

            history.Add(packet);
        }

        return history;
    }
}