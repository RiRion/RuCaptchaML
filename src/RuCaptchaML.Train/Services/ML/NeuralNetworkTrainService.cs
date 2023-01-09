using System.Diagnostics;
using Common;
using Microsoft.ML;
using Microsoft.ML.Transforms;
using RuCaptchaML.Shared;
using RuCaptchaML.Shared.Models.Configs;
using RuCaptchaML.Train.Models;
using RuCaptchaML.Train.Models.Configs;

namespace RuCaptchaML.Train.Services.ML;

public class NeuralNetworkTrainService : INeuralNetworkTrainService
{
    private readonly DatasetConfig _datasetConfig;
    private readonly NeuralNetworkConfig _neuralNetworkConfig;
    private readonly MLContext _mlContext;

    public NeuralNetworkTrainService(DatasetConfig datasetConfig, NeuralNetworkConfig neuralNetworkConfig, MLContext mlContext)
    {
        _datasetConfig = datasetConfig;
        _neuralNetworkConfig = neuralNetworkConfig;
        _mlContext = mlContext;
    }

    public void Train()
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        string fullImagesetFolderPath = Path.Combine(currentDirectory, _datasetConfig.SaveDirectory);
        string fullPathSaveMLModel = Path.Combine(currentDirectory, _neuralNetworkConfig.Path);

        FileUtils.CreateDirectoryIfNotExist(Path.Combine(currentDirectory, _neuralNetworkConfig.MlNetModelDirectory));
        FileUtils.DeleteFileIfExist(fullPathSaveMLModel);

        _mlContext.Log += FilterMLContextLog;

        IEnumerable<ImageData> images = LoadImagesFromDirectory(folder: fullImagesetFolderPath);
        IDataView fullImagesDataset = _mlContext.Data.LoadFromEnumerable(images);
        IDataView shuffledFullImageFilePathsDataset = _mlContext.Data.ShuffleRows(fullImagesDataset);

        IDataView shuffledFullImagesDataset = _mlContext.Transforms.Conversion.
            MapValueToKey(outputColumnName: "LabelAsKey", inputColumnName: "Label", keyOrdinality: ValueToKeyMappingEstimator.KeyOrdinality.ByValue)
            .Append(_mlContext.Transforms.LoadRawImageBytes(
                outputColumnName: "Image",
                imageFolder: fullImagesetFolderPath,
                inputColumnName: "ImagePath"))
            .Fit(shuffledFullImageFilePathsDataset)
            .Transform(shuffledFullImageFilePathsDataset);

        var trainTestData = _mlContext.Data.TrainTestSplit(shuffledFullImagesDataset, testFraction: 0.2);
        IDataView trainDataView = trainTestData.TrainSet;
        IDataView testDataView = trainTestData.TestSet;

        var pipeline = _mlContext.MulticlassClassification.Trainers
            .ImageClassification(featureColumnName: "Image",
                labelColumnName: "LabelAsKey",
                validationSet: testDataView)
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue(outputColumnName: "PredictedLabel",
                inputColumnName: "PredictedLabel"));

        Console.WriteLine("*** Training the image classification model with DNN Transfer Learning on top of the selected pre-trained model/architecture ***");

        // Measuring training time
        var watch = Stopwatch.StartNew();

        //Train
        ITransformer trainedModel = pipeline.Fit(trainDataView);

        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;

        Console.WriteLine($"Training with transfer learning took: {elapsedMs / 1000} seconds");

        EvaluateModel(_mlContext, testDataView, trainedModel);

        _mlContext.Model.Save(trainedModel, trainDataView.Schema, fullPathSaveMLModel);
        Console.WriteLine($"Model saved to: {fullPathSaveMLModel}");
    }

    private static void EvaluateModel(MLContext mlContext, IDataView testDataset, ITransformer trainedModel)
    {
        Console.WriteLine("Making predictions in bulk for evaluating model's quality...");

        // Measuring time
        var watch = Stopwatch.StartNew();

        var predictionsDataView = trainedModel.Transform(testDataset);

        var metrics = mlContext.MulticlassClassification.Evaluate(predictionsDataView, labelColumnName:"LabelAsKey", predictedLabelColumnName: "PredictedLabel");
        ConsoleHelper.PrintMultiClassClassificationMetrics("TensorFlow DNN Transfer Learning", metrics);

        watch.Stop();
        var elapsed2Ms = watch.ElapsedMilliseconds;

        Console.WriteLine($"Predicting and Evaluation took: {elapsed2Ms / 1000} seconds");
    }

    public static IEnumerable<ImageData> LoadImagesFromDirectory(
        string folder,
        bool useFolderNameAsLabel = true)
        => FileUtils.LoadImagesFromDirectory(folder, useFolderNameAsLabel)
            .Select(x => new ImageData(x.imagePath, x.label));

    private static void FilterMLContextLog(object sender, LoggingEventArgs e)
    {
        if (e.Message.StartsWith("[Source=ImageClassificationTrainer;"))
        {
            Console.WriteLine(e.Message);
        }
    }
}
