using Microsoft.AspNetCore.Mvc;
using SmartX.Server.Models;
using SmartX.Server.Services;
using SmartX.Shared.Models;

namespace SmartX.Server.Controllers;

[ApiController]
[Route("api/fuel-prediction")]
public sealed class FuelPredictionController : ControllerBase
{
    private readonly FuelPredictionService _predictionService;

    public FuelPredictionController(FuelPredictionService predictionService)
    {
        _predictionService = predictionService;
    }

    [HttpPost("range")]
    public ActionResult<FuelRangePrediction> PredictRange([FromBody] TruckTelemetry telemetry)
    {
        if (!IsValidInput(telemetry))
        {
            return BadRequest("Truck ID and finite, non-negative speed, distance, fuel, and payload values are required.");
        }

        var input = new FuelInputData
        {
            SpeedKmh = telemetry.SpeedKmh,
            DistanceTraveledKm = (float)telemetry.DistanceTraveledKm,
            CurrentFuelLiters = telemetry.FuelLevelLiters,
            PayloadWeightTons = telemetry.PayloadWeightTons
        };
        var predictedRangeKm = _predictionService.PredictRange(input);
        if (!float.IsFinite(predictedRangeKm))
        {
            return Problem("The fuel range model returned an invalid prediction.");
        }

        return Ok(new FuelRangePrediction
        {
            TruckId = telemetry.TruckId,
            PredictedRemainingRangeKm = Math.Max(0, predictedRangeKm),
            PredictedAtUtc = DateTime.UtcNow
        });
    }

    private static bool IsValidInput(TruckTelemetry telemetry) =>
        !string.IsNullOrWhiteSpace(telemetry.TruckId) &&
        float.IsFinite(telemetry.SpeedKmh) && telemetry.SpeedKmh >= 0 &&
        double.IsFinite(telemetry.DistanceTraveledKm) && telemetry.DistanceTraveledKm is >= 0 and <= float.MaxValue &&
        float.IsFinite(telemetry.FuelLevelLiters) && telemetry.FuelLevelLiters >= 0 &&
        float.IsFinite(telemetry.PayloadWeightTons) && telemetry.PayloadWeightTons >= 0;
}
