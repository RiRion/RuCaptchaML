using RuCaptchaML.Shared.Dataset;
using RuCaptchaML.Train.Models.Configs;

namespace RuCaptchaML.Train.Services.Dataset;

public class DatasetService : IDatasetService
{
    private readonly DatasetConfig _datasetConfig;

    public DatasetService(DatasetConfig datasetConfig)
    {
        _datasetConfig = datasetConfig;
    }

    public void CreateDatasetFromImages()
    {
        var inputImagesPath = Path.Combine(Directory.GetCurrentDirectory(), _datasetConfig.InputImages);
        var outputDatasetPath = Path.Combine(Directory.GetCurrentDirectory(), _datasetConfig.SaveDirectory);

        DatasetUtils.ParseImageForDataset(inputImagesPath, outputDatasetPath);
    }
}
