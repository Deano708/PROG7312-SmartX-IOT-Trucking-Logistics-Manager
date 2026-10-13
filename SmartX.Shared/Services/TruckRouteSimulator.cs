using SmartX.Shared.Models;

namespace SmartX.Shared.Services;

public static class TruckRouteSimulator
{
    private const int HistoryHours = 12;

    private static readonly IReadOnlyDictionary<string, GeoPoint[]> Routes = new Dictionary<string, GeoPoint[]>
    {
        ["TRK-001"] = [new(-26.2041, 28.0473), new(-28.2300, 28.3100), new(-29.6000, 30.3800), new(-29.8587, 31.0218)],
        ["TRK-002"] = [new(-29.8587, 31.0218), new(-29.6000, 30.3800), new(-28.2300, 28.3100), new(-26.2041, 28.0473)],
        ["TRK-003"] = [new(-26.2041, 28.0473), new(-27.6500, 27.2300), new(-29.1181, 26.2243), new(-32.3500, 22.5800), new(-33.9249, 18.4241)],
        ["TRK-004"] = [new(-33.9249, 18.4241), new(-34.1800, 22.1400), new(-34.0400, 23.0500), new(-33.9608, 25.6022)],
        ["TRK-005"] = [new(-26.2041, 28.0473), new(-25.7460, 28.1881), new(-24.8800, 28.1900), new(-24.1900, 29.0100), new(-23.9045, 29.4688)],
        ["TRK-006"] = [new(-26.2041, 28.0473), new(-27.6500, 27.2300), new(-27.9800, 26.7300), new(-29.1181, 26.2243)],
        ["TRK-007"] = [new(-26.2041, 28.0473), new(-26.1000, 27.7800), new(-25.8500, 27.3000), new(-25.6544, 27.2559)],
        ["TRK-008"] = [new(-29.8587, 31.0218), new(-30.7400, 30.4500), new(-31.5900, 28.7800), new(-33.0153, 27.8935)],
        ["TRK-009"] = [new(-26.2041, 28.0473), new(-25.8700, 29.2300), new(-25.6700, 30.2500), new(-25.4753, 30.9694)],
        ["TRK-010"] = [new(-26.2041, 28.0473), new(-26.7100, 27.1000), new(-26.8500, 26.6700), new(-28.7282, 24.7499)],
        ["TRK-011"] = [new(-29.8587, 31.0218), new(-29.3300, 31.2900), new(-28.7807, 32.0383)],
        ["TRK-012"] = [new(-28.7282, 24.7499), new(-28.9000, 23.6000), new(-28.9000, 21.9800), new(-28.4478, 21.2561)]
    };

    public static List<TelemetryPacket<TruckTelemetry>> GenerateHourlySamples(
        IEnumerable<TruckTelemetry> fleet,
        DateTime? currentTime = null)
    {
        var latestHour = (currentTime ?? DateTime.UtcNow).ToUniversalTime();
        latestHour = new DateTime(latestHour.Ticks - latestHour.Ticks % TimeSpan.TicksPerHour, DateTimeKind.Utc);
        var samples = new List<TelemetryPacket<TruckTelemetry>>();

        foreach (var truck in fleet)
        {
            if (!Routes.TryGetValue(truck.TruckId, out var route))
            {
                continue;
            }

            var cumulativeDistances = GetCumulativeDistances(route);
            for (var hoursAgo = HistoryHours; hoursAgo >= 0; hoursAgo--)
            {
                var position = GetPositionAtFraction(route, cumulativeDistances, (double)(HistoryHours - hoursAgo) / HistoryHours);
                var telemetry = truck;
                telemetry.Latitude = position.Latitude;
                telemetry.Longitude = position.Longitude;
                samples.Add(new TelemetryPacket<TruckTelemetry>(GetDeviceMac(truck.TruckId), telemetry)
                {
                    Timestamp = latestHour.AddHours(-hoursAgo)
                });
            }
        }

        return samples;
    }

    private static double[] GetCumulativeDistances(IReadOnlyList<GeoPoint> route)
    {
        var distances = new double[route.Count];
        for (var i = 1; i < route.Count; i++)
        {
            var latitudeDifference = route[i].Latitude - route[i - 1].Latitude;
            var longitudeDifference = (route[i].Longitude - route[i - 1].Longitude) * Math.Cos(route[i - 1].Latitude * Math.PI / 180);
            distances[i] = distances[i - 1] + Math.Sqrt(latitudeDifference * latitudeDifference + longitudeDifference * longitudeDifference);
        }

        return distances;
    }

    private static GeoPoint GetPositionAtFraction(IReadOnlyList<GeoPoint> route, IReadOnlyList<double> cumulativeDistances, double fraction)
    {
        var targetDistance = cumulativeDistances[^1] * fraction;
        for (var i = 1; i < route.Count; i++)
        {
            if (targetDistance <= cumulativeDistances[i])
            {
                var segmentFraction = (targetDistance - cumulativeDistances[i - 1]) / (cumulativeDistances[i] - cumulativeDistances[i - 1]);
                return new GeoPoint(
                    route[i - 1].Latitude + ((route[i].Latitude - route[i - 1].Latitude) * segmentFraction),
                    route[i - 1].Longitude + ((route[i].Longitude - route[i - 1].Longitude) * segmentFraction));
            }
        }

        return route[^1];
    }

    private static string GetDeviceMac(string truckId) => $"02:42:AC:11:00:{truckId[^2..]}";

    private readonly record struct GeoPoint(double Latitude, double Longitude);
}
