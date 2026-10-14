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
        var trainingData = new List<FuelInputData>
    {
        // Fully laden (32 tons payload) - Heavy climb/corridor conditions (High fuel consumption ~50 L/100km)
        new() { SpeedKmh = 72.0f, DistanceTraveledKm = 150f, CurrentFuelLiters = 750f, PayloadWeightTons = 32.0f, RemainingRangeKm = 1500f },
        new() { SpeedKmh = 68.0f, DistanceTraveledKm = 450f, CurrentFuelLiters = 500f, PayloadWeightTons = 32.0f, RemainingRangeKm = 1000f },
        new() { SpeedKmh = 65.0f, DistanceTraveledKm = 850f, CurrentFuelLiters = 250f, PayloadWeightTons = 32.0f, RemainingRangeKm = 500f },
        new() { SpeedKmh = 60.0f, DistanceTraveledKm = 1050f, CurrentFuelLiters = 95f, PayloadWeightTons = 32.0f, RemainingRangeKm = 190f },

        // Moderately laden (24 tons payload) - Normal highway cruising (~42 L/100km)
        new() { SpeedKmh = 82.0f, DistanceTraveledKm = 200f, CurrentFuelLiters = 680f, PayloadWeightTons = 24.0f, RemainingRangeKm = 1619f },
        new() { SpeedKmh = 80.0f, DistanceTraveledKm = 600f, CurrentFuelLiters = 410f, PayloadWeightTons = 24.0f, RemainingRangeKm = 976f },
        new() { SpeedKmh = 78.0f, DistanceTraveledKm = 900f, CurrentFuelLiters = 180f, PayloadWeightTons = 24.0f, RemainingRangeKm = 428f },

        // Lightly laden / Inter-link empty (15 tons payload) - Optimal efficiency (~35 L/100km)
        new() { SpeedKmh = 88.0f, DistanceTraveledKm = 300f, CurrentFuelLiters = 700f, PayloadWeightTons = 15.0f, RemainingRangeKm = 2000f },
        new() { SpeedKmh = 85.0f, DistanceTraveledKm = 700f, CurrentFuelLiters = 450f, PayloadWeightTons = 15.0f, RemainingRangeKm = 1285f },
        new() { SpeedKmh = 85.0f, DistanceTraveledKm = 1050f, CurrentFuelLiters = 120f, PayloadWeightTons = 15.0f, RemainingRangeKm = 342f }
    };

        IDataView dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

        var pipeline = _mlContext.Transforms.Concatenate("Features",
                nameof(FuelInputData.SpeedKmh),
                nameof(FuelInputData.DistanceTraveledKm),
                nameof(FuelInputData.CurrentFuelLiters),
                nameof(FuelInputData.PayloadWeightTons))
            .Append(_mlContext.Regression.Trainers.FastTree(labelColumnName: nameof(FuelInputData.RemainingRangeKm)));

        return pipeline.Fit(dataView);
    }

    public float PredictRange(FuelInputData input)
    {
        var predictionEngine = _mlContext.Model.CreatePredictionEngine<FuelInputData, FuelPrediction>(_model);
        var prediction = predictionEngine.Predict(input);
        return prediction.PredictedRemainingRangeKm;
    }
}