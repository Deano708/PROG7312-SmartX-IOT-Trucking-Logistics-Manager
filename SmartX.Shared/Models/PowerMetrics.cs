namespace SmartX.Shared.Models;
//makes use of Operator Overloading to encapsulate power metrics for fleet vehicles and IoT devices.
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