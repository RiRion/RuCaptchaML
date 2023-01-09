namespace RuCaptcha.Predict.Services.ML;

public interface INeuralNetworkPredictService
{
    string Predict(byte[] image);
}
