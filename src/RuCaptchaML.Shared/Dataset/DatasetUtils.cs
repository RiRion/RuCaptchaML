using OpenCvSharp;
using RuCaptchaML.Shared.Dataset.Exceptions;
using RuCaptchaML.Shared.Dataset.Extensions;
using RuCaptchaML.Shared.Models;

namespace RuCaptchaML.Shared.Dataset;

public static class DatasetUtils
{
    /// <summary>
    /// Распознает символы, представленные на изображениях.
    /// Сохраняет распознанные символы в виде изображений в соответствующем символу каталоге в заданной директории.
    /// </summary>
    /// <param name="inputImagesPath">путь к каталогу изображений.</param>
    /// <param name="outputDatasetPath">каталог в который будет сохранен Dataset.</param>
    public static void ParseImageForDataset(string inputImagesPath, string outputDatasetPath)
    {
        var files = Directory.EnumerateFiles(inputImagesPath).ToList();

        var rawImages = 0;

        foreach (var file in files)
        {
            if (!ParseImageForDatasetProcessing(file, outputDatasetPath)) rawImages++;
        }

        Console.WriteLine($"Обработано изображений {files.Count - rawImages} из {files.Count}");
    }

    /// <summary>
    /// Распознает символы, представленные на изображении.
    /// </summary>
    /// <param name="bytes">изображение в виде массива byte</param>
    /// <param name="charsCount">количество символов на изображении.</param>
    /// <returns>Список распознанных изображений прдставленных объектом типа <see cref="InMemoryImageData"/>, упорядоченный по расположению символов.</returns>
    /// <exception cref="ImageNotRecognizedException">Изображение не распознано.</exception>
    public static IReadOnlyList<InMemoryImageData> ParseImage(byte[] bytes, int charsCount)
    {
        using var sourceImage = GetImage(bytes);

        var rects = GetRects(sourceImage);

        if (!CheckRects(ref rects, charsCount)) throw new ImageNotRecognizedException("Image not recognized.");

        return rects.Select(e =>
            {
                using var mat = sourceImage.GetMatFromRect(e);
                return new InMemoryImageData(mat.ToBytes(), null, null);
            })
            .ToList();
    }

    /// <summary>
    /// Распознает символы, представленные на изображении.
    /// Сохраняет распознанные символы в виде изображений в соответствующем символу каталоге в заданной директории.
    /// </summary>
    /// <param name="inputImagePath">путь к файлу изображения.</param>
    /// <param name="outputDatasetPath">каталог в который будет сохранен Dataset.</param>
    /// <returns>true - если изображение распознано успешно, иначе - false.</returns>
    private static bool ParseImageForDatasetProcessing(string inputImagePath, string outputDatasetPath)
    {
        var chars = Path.GetFileNameWithoutExtension(inputImagePath).ToArray();

        using var sourceImage = GetImage(inputImagePath);

        var rects = GetRects(sourceImage);

        if (!CheckRects(ref rects, chars)) return false;

        for (var i = 0; i < rects.Count; i++)
        {
            var saveDirectory = Path.Combine(outputDatasetPath, chars[i].ToString());
            using var image = sourceImage.GetMatFromRect(rects[i]);
            SaveItemImage(saveDirectory, $"{Guid.NewGuid()}.png", image);
        }

        return true;
    }

    /// <summary>
    /// Получает изображение в нормализованном (едином формате) состоянии.
    /// </summary>
    /// <param name="path">путь к файлу изображения</param>
    /// <returns><see cref="Mat"/> объект, представляющий изображение</returns>
    private static Mat GetImage(string path)
    {
        // TODO: очистка шумов https://stackoverflow.com/questions/59220141/c-sharp-how-to-use-opencv-to-remove-noise-of-a-captcha

        var sourceImage = new Mat(path, ImreadModes.Grayscale);

        sourceImage = BringToFormat(sourceImage);

        return sourceImage;
    }

    /// <summary>
    /// Получает изображение в нормализованном (едином формате) состоянии.
    /// </summary>
    /// <param name="bytes">изображение в виде массива byte</param>
    /// <returns><see cref="Mat"/> объект, представляющий изображение</returns>
    private static Mat GetImage(byte[] bytes)
    {
        var sourceImage = Cv2.ImDecode(bytes, ImreadModes.Grayscale);

        sourceImage = BringToFormat(sourceImage);

        return sourceImage;
    }

    private static Mat BringToFormat(Mat sourceImage)
    {
        // добавление бордера
        Cv2.CopyMakeBorder(sourceImage, sourceImage, 8, 8, 8, 8, BorderTypes.Replicate);

        // Преобразование в черно-белый цвет и какая то магия связанная с типами, без последнего не определяет границы контура
        Cv2.Threshold(sourceImage, sourceImage, 0, 255, ThresholdTypes.BinaryInv | ThresholdTypes.Otsu);

        return sourceImage;
    }

    /// <summary>
    /// Список координат областей, обнаруженных символов.
    /// </summary>
    /// <param name="image"><see cref="Mat"/> объект изображения, для которого осуществляется поиск символов.</param>
    /// <returns>Список областей типа <see cref="Rect"/>.</returns>
    private static List<Rect> GetRects(Mat image)
    {
        var contours = Cv2.FindContoursAsMat(image, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

        // Список координат областей обнаруженных символов
        return contours
            .Select(e => Cv2.BoundingRect(e).AddMargin(image, 1, 1, 8, 1))
            .Where(e => e.Height > 20)
            .OrderBy(e => e.X)
            .ToList();
    }

    /// <summary>
    /// Проверка на совпадение количесва символов и сформированных областей изображений.
    /// В случае не совпадения - попытается исправить коллекцию областей один раз.
    /// </summary>
    /// <param name="rects">обнаруженные области символов на изображении.</param>
    /// <param name="chars">символы, содержащиеся на изображении.</param>
    /// <returns>true - если количесво областей совпадает с количеством символов, иначе - false.</returns>
    private static bool CheckRects(ref List<Rect> rects, char[] chars)
    {
        return CheckRects(ref rects, chars.Length);
    }

    private static bool CheckRects(ref List<Rect> rects, int charCount)
    {
        if (CheckRectByCharCount(rects.Count, charCount)) return true;
        rects = SplitRectsByWidth(rects);
        return CheckRectByCharCount(rects.Count, charCount);
    }

    private static bool CheckRectByCharCount(int rectCount, int charCount) => rectCount.Equals(charCount);

    private static List<Rect> SplitRectsByWidth(List<Rect> rects)
    {
        var currentRects = new List<Rect>();
        for (var i = 0; i < rects.Count; i++)
        {
            var rect = rects[i];
            var r = (double)rect.Width / rect.Height;
            if ((double)rect.Width / rect.Height > 1.0)
            {
                rect.Width /= 2;
                var secondRect = new Rect(rect.X + rect.Width, rect.Y, rect.Width, rect.Height);
                currentRects.Add(secondRect);
            }
            currentRects.Add(rect);
        }

        return currentRects.OrderBy(e => e.X).ToList();
    }

    private static void SaveItemImage(string path, string fileName, Mat image)
    {
        var savePath = Path.Combine(path, fileName);
        FileUtils.CreateDirectoryIfNotExist(path);
        FileUtils.DeleteFileIfExist(savePath);
        Cv2.ImWrite(savePath, image);
    }

    /// <summary>
    /// Отображает полученне изображение в новом окне.
    /// </summary>
    /// <param name="name">название окна.</param>
    /// <param name="img">изображение, представленное типом <see cref="Mat"/>.</param>
    private static void Show(string name, Mat img)
    {
        Cv2.ImShow(name, img);
        Cv2.WaitKey();
        Cv2.DestroyAllWindows();
    }
}
