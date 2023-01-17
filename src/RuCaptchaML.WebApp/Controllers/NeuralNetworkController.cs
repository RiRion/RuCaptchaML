using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using RuCaptcha.Predict.Services.ML;
using RuCaptchaML.Shared.ImageHelpers;

namespace RuCaptchaML.WebApp.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class NeuralNetworkController : ControllerBase
{
    private readonly ILogger<NeuralNetworkController> _logger;
    private readonly INeuralNetworkPredictService _neuralNetworkPredictService;

    public NeuralNetworkController(ILogger<NeuralNetworkController> logger, INeuralNetworkPredictService neuralNetworkPredictService)
    {
        _logger = logger;
        _neuralNetworkPredictService = neuralNetworkPredictService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    public ActionResult<string> Predict(IFormFile imageFile)
    {
        if (imageFile.Length == 0) return BadRequest();

        var imageData = GetByteArrayFromIFormFile(imageFile);
        if (!imageData.IsValidImage())
            return StatusCode(StatusCodes.Status415UnsupportedMediaType);
        
        try
        {
            return _neuralNetworkPredictService.Predict(imageData);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static byte[] GetByteArrayFromIFormFile(IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        file.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}
