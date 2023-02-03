namespace RuCaptchaML.Predict.Services.ML;

public interface INeuralNetworkPredictService
{
    string Predict(byte[] image);
}
