using Microsoft.ML.Data;

namespace RuCaptchaML.Shared.Models
{
    public class ImagePrediction
    {
        [ColumnName("Score")]
        public float[] Score;

        [ColumnName("PredictedLabel")]
        public string PredictedLabel;
    }
}
