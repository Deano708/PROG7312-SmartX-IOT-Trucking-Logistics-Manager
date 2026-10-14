using Microsoft.ML;
using System.Data;

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
        // In production, load this from a CSV file or database history
        var trainingData = new List<FuelInputData>
        {
            new() { SpeedKmh = 82.5f, DistanceTraveledKm = 420f, CurrentFuelLiters = 310f, PayloadWeightTons = 24f, RemainingRangeKm = 680f },
            new() { SpeedKmh = 65.0f, DistanceTraveledKm = 1150f, CurrentFuelLiters = 85f, PayloadWeightTons = 32f, RemainingRangeKm = 140f },
            new() { SpeedKmh = 88.0f, DistanceTraveledKm = 620f, CurrentFuelLiters = 210f, PayloadWeightTons = 18f, RemainingRangeKm = 450f }
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