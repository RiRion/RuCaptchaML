namespace RuCaptchaML.Shared.Models.Configs;

public class NeuralNetworkConfig
{
    public string MlNetModelDirectory { get; set; }

    public string MLModelFileName { get; set; }

    public string Path => System.IO.Path.Combine(MlNetModelDirectory, MLModelFileName);
}
