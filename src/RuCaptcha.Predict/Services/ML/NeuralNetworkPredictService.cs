using Microsoft.Extensions.ML;
using Microsoft.ML;
using RuCaptchaML.Shared.Dataset;
using RuCaptchaML.Shared.Models;

namespace RuCaptcha.Predict.Services.ML;

public class NeuralNetworkPredictService : INeuralNetworkPredictService
{
    private readonly PredictionEnginePool<InMemoryImageData, ImagePrediction> _predictionEnginePool;

    public NeuralNetworkPredictService(PredictionEnginePool<InMemoryImageData, ImagePrediction> predictionEnginePool)
    {
        _predictionEnginePool = predictionEnginePool;
    }

    public string Predict(byte[] image)
    {
        int charCount = 5;

        var listOfImageData = DatasetUtils.ParseImage(image, charCount);

        ImagePrediction[] outputList = new ImagePrediction[charCount];

        for (int i = 0; i < charCount; i++)
        {
            outputList[i] = _predictionEnginePool.Predict(listOfImageData[i]);
        }

        return string.Concat(outputList.Select(e => e.PredictedLabel));
    }
}
