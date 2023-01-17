using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ML;
using RuCaptcha.Predict.Services.ML;
using RuCaptchaML.Shared.Models;

namespace RuCaptcha.Predict.DependencyInjection;

public static class PredictRegistration
{
    public static IServiceCollection AddCaptchaMLPredict(this IServiceCollection services, string mlModelPath)
    {
        if (!File.Exists(mlModelPath)) throw new FileNotFoundException();
        
        services.AddPredictionEnginePool<InMemoryImageData, ImagePrediction>()
            .FromFile(mlModelPath);

        services.AddScoped<INeuralNetworkPredictService, NeuralNetworkPredictService>();
        
        return services;
    }
}