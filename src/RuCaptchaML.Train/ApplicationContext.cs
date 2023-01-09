using RuCaptchaML.Train.Services.Dataset;
using RuCaptchaML.Train.Services.ML;

namespace RuCaptchaML.Train;

public class ApplicationContext
{
    private readonly IDatasetService _datasetService;
    private readonly INeuralNetworkTrainService _neuralNetworkTrainService;

    public ApplicationContext(IDatasetService datasetService, INeuralNetworkTrainService neuralNetworkTrainService)
    {
        _datasetService = datasetService;
        _neuralNetworkTrainService = neuralNetworkTrainService;
    }

    public async Task RunAsync()
    {
        _datasetService.CreateDatasetFromImages();

        _neuralNetworkTrainService.Train();
    }
}
