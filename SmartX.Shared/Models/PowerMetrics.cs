namespace SmartX.Shared.Models;

public struct PowerMetric
{
    public int LoadWatts { get; set; }

    public static PowerMetric operator +(PowerMetric a, PowerMetric b) =>
        new PowerMetric { LoadWatts = a.LoadWatts + b.LoadWatts };

    public static bool operator >(PowerMetric a, PowerMetric b) =>
        a.LoadWatts > b.LoadWatts;

    public static bool operator <(PowerMetric a, PowerMetric b) =>
        a.LoadWatts < b.LoadWatts;
}