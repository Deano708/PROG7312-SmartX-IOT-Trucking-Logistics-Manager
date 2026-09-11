using SmartX.Shared.Models;

namespace SmartX.Shared.Services;

public class TelemetryAggregator
{
    public PowerMetric CombineMeterLoads(PowerMetric meter1, PowerMetric meter2)
    {
        return meter1 + meter2;
    }

    public TruckTelemetry MergeTruckMetrics(TruckTelemetry truck1, TruckTelemetry truck2)
    {
        return truck1 + truck2;
    }

    public bool IsOverloaded(PowerMetric current, PowerMetric threshold)
    {
        return current > threshold;
    }
}