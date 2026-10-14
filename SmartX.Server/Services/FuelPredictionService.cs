using Microsoft.ML;
using SmartX.Server.Models;

namespace SmartX.Server.Services;

public class FuelPredictionService
{
    private readonly MLContext _mlContext;
    private ITransformer _model;

    public FuelPredictionService()
    {
        _mlContext = new MLContext();
        _model = TrainModel();
    }

    private ITransformer TrainModel()
    {
        var trainingData = GenerateTrainingData();

        IDataView dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

        var pipeline = _mlContext.Transforms.Concatenate("Features",
                nameof(FuelInputData.SpeedKmh),
                nameof(FuelInputData.CurrentFuelLiters),
                nameof(FuelInputData.PayloadWeightTons))
            .Append(_mlContext.Regression.Trainers.FastTree(labelColumnName: nameof(FuelInputData.RemainingRangeKm)));

        return pipeline.Fit(dataView);
    }

    private static List<FuelInputData> GenerateTrainingData()
    {
        float[] speedsKmh = [50f, 55f, 60f, 65f, 68f, 72f, 75f, 78f, 80f, 82f, 85f, 88f, 90f, 95f];
        float[] fuelLevelsLiters = [0f, 50f, 85f, 95f, 120f, 180f, 190f, 210f, 230f, 250f, 260f, 280f, 300f, 310f, 340f, 390f, 410f, 450f, 500f, 600f, 680f, 700f, 750f, 800f];
        float[] payloadWeightsTons = [15f, 24f, 32f];
        var trainingData = new List<FuelInputData>(speedsKmh.Length * fuelLevelsLiters.Length * payloadWeightsTons.Length);

        foreach (var speedKmh in speedsKmh)
        {
            foreach (var fuelLiters in fuelLevelsLiters)
            {
                foreach (var payloadTons in payloadWeightsTons)
                {
                    var fuelConsumptionLitersPer100Km = EstimateFuelConsumption(speedKmh, payloadTons);
                    trainingData.Add(new FuelInputData
                    {
                        SpeedKmh = speedKmh,
                        CurrentFuelLiters = fuelLiters,
                        PayloadWeightTons = payloadTons,
                        RemainingRangeKm = fuelConsumptionLitersPer100Km <= 0
                            ? 0
                            : fuelLiters * 100f / fuelConsumptionLitersPer100Km
                    });
                }
            }
        }

        return trainingData;
    }

    private static float EstimateFuelConsumption(float speedKmh, float payloadTons)
    {
        var payloadConsumption = 35f + ((payloadTons - 15f) * (15f / 17f));
        var speedPenalty = Math.Abs(speedKmh - 80f) * 0.1f;
        return payloadConsumption + speedPenalty;
    }

    public float PredictRange(FuelInputData input)
    {
        if (input.SpeedKmh <= 0.5f || input.CurrentFuelLiters <= 0f)
        {
            return 0f;
        }

        var predictionEngine = _mlContext.Model.CreatePredictionEngine<FuelInputData, FuelPrediction>(_model);
        var prediction = predictionEngine.Predict(input);
        return float.IsFinite(prediction.PredictedRemainingRangeKm)
            ? Math.Max(0f, prediction.PredictedRemainingRangeKm)
            : 0f;
    }
}