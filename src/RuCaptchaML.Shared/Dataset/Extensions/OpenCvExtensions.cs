using OpenCvSharp;

namespace RuCaptchaML.Shared.Dataset.Extensions;

public static class OpenCvExtensions
{
    public static Rect AddMargin(this Rect rect, Mat sourceImage, int bottom, int right, int top, int left)
    {
        bottom = sourceImage.Height < rect.Height + rect.Y + bottom ? sourceImage.Height - rect.Y - rect.Height : bottom;
        top = rect.Y - top < 0 ? rect.Y : top;

        rect.X -= right;
        rect.Y -= top;
        rect.Height += top + bottom;
        rect.Width += left + right;

        return rect;
    }

    public static Rect AddMargin(this Rect rect, int margin)
    {
        rect.X -= margin;
        rect.Y -= margin;
        rect.Height += margin * 2;
        rect.Width += margin * 2;

        return rect;
    }

    public static Mat GetMatFromRect(this Mat source, Rect rect)
    {
        var image = source[rect];
        Cv2.CvtColor(image, image, ColorConversionCodes.GRAY2RGB);
        Cv2.Threshold(image, image, 0, 255, ThresholdTypes.BinaryInv);
        return image;
    }
}
