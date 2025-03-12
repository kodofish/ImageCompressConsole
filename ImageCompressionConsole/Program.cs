using CommandLine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace ImageCompressionConsole;

public class Options
{
    [Option('s', "source", Required = true, Default = null, HelpText = "來源目錄，包含要壓縮的圖片檔案")]
    public string SourceDirectory { get; set; }

    [Option('o', "output", Required = true, Default = null, HelpText = "輸出目錄，壓縮後的圖片存放位置")]
    public string OutputDirectory { get; set; }

    [Option('c', "count", Required = false, Default = 100, HelpText = "要處理的圖片數量, 預設為 100")]
    public int Count { get; set; }

    [Option('e', "size", Required = false, Default = 5, HelpText = "要處理的圖片大小限制, 預設為 5MB")]
    public double FileSizeLimit { get; set; }
}

static class Program
{
    private const int MaxWidth = 1320;

    /// <summary>
    /// An instance of the PngEncoder class, used to configure settings for encoding PNG images.
    /// </summary>
    /// <remarks>
    /// This instance is pre-configured with a compression level of Level5, which offers a balance
    /// between file size and encoding performance. It is used to apply lossless compression
    /// to PNG images within the application.
    /// </remarks>
    private static readonly PngEncoder PngEncoder = new() { CompressionLevel = PngCompressionLevel.BestCompression };

    /// <summary>
    /// An instance of the JpegEncoder class, used to configure settings for encoding JPEG images.
    /// </summary>
    /// <remarks>
    /// This instance is pre-configured with a quality setting of 75, which strikes a balance between
    /// image quality and file size. It is used to apply lossy compression to JPEG images within the application.
    /// </remarks>
    private static readonly JpegEncoder JpegEncoder = new() { Quality = 75 };

    /// <summary>
    /// An instance of the WebpEncoder class, used to configure settings for encoding WEBP images.
    /// </summary>
    /// <remarks>
    /// This instance is configured with a quality setting of 75, providing a balance between image quality and file size.
    /// It applies lossy compression to WEBP images and uses the default encoding method for optimal performance.
    /// </remarks>
    private static readonly WebpEncoder WebpEncoder = new()
    {
        Quality = 75, // 壓縮品質（0-100 範圍），壓縮品質越低，文件越小但畫質越差
        Method = WebpEncodingMethod.Default // 編碼方法（可選），默認為標準
    };

    /// <summary>
    /// A collection of supported image file extensions utilized to filter image files for processing.
    /// </summary>
    /// <remarks>
    /// This array contains the list of file extensions the application recognizes as valid image formats
    /// for operations such as compression or processing. It ensures only files with supported extensions
    /// are included in the workflow.
    /// </remarks>
    private static readonly string[] SupportedImageExtensions = [".jpg", ".png", ".webp"];

    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(ProcessDirectoryOptions)
            .WithNotParsed(errors =>
            {
                Console.WriteLine("Parameter parsing failed, please check if the command format is correct.\n");
                Environment.Exit(1);
            });
    }

    private static HashSet<string> LoadProcessedFiles(string recordFilePath)
    {
        return File.Exists(recordFilePath)
            ? new HashSet<string>(File.ReadAllLines(recordFilePath))
            : new HashSet<string>();
    }

    /// <summary>
    /// Saves the path of a processed file to a record file, appending it as a new line.
    /// </summary>
    /// <param name="recordFilePath">The path to the file used to store records of processed file paths.</param>
    /// <param name="filePath">The path of the file that has been processed.</param>
    private static void SaveProcessedFile(string recordFilePath, string filePath)
    {
        File.AppendAllText(recordFilePath, filePath + Environment.NewLine);
    }


    private static void ProcessDirectoryOptions(Options options)
    {
        var sourceDirectory = options.SourceDirectory;
        var outputDirectory = options.OutputDirectory;
        var recordFilePath = Path.Combine(outputDirectory, "processed_files.txt"); // 紀錄檔案的路徑
        var maxFilesToProcess = options.Count;
        var fileSizeLimit = options.FileSizeLimit * 1024 * 1024;

        if (!Directory.Exists(sourceDirectory))
        {
            Console.WriteLine($"來源目錄不存在: {sourceDirectory}");
            return;
        }

        if (!Directory.Exists(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        var processedFiles = LoadProcessedFiles(recordFilePath);

        var imageFiles = Directory.GetFiles(sourceDirectory, "*.*")
            .Where(file => SupportedImageExtensions.Contains(Path.GetExtension(file).ToLower()))
            .Where(file => !processedFiles.Contains(file))
            .Select(file => new { Path = file, Size = new FileInfo(file).Length })
            .Where(files => files.Size > fileSizeLimit)
            .OrderByDescending(file => file.Size)
            .AsEnumerable();

        Console.WriteLine($"開始處理圖檔...總計 {imageFiles.Count()} 個圖檔，最多處理 {maxFilesToProcess} 個圖檔\n");

        imageFiles = imageFiles.Take(maxFilesToProcess);

        // 如果沒有圖檔，則不進行處理
        if (!imageFiles.Any())
        {
            Console.WriteLine("沒有圖檔需要處理");
            return;
        }

        foreach (var filePath in imageFiles.Select(file => file.Path))
        {
            ImageCompressionProcess(filePath, outputDirectory, MaxWidth);
            SaveProcessedFile(recordFilePath, filePath);
        }

        Console.WriteLine("所有圖檔已完成處理");
    }

    /// <summary>
    /// 壓縮指定的圖片檔案，並將結果存儲到指定的輸出目錄中。
    /// </summary>
    /// <param name="filePath">要壓縮的圖片檔案路徑。</param>
    /// <param name="outputDirectory">存放壓縮後圖片檔案的目錄。</param>
    /// <param name="maxWidth">壓縮圖片的最大寬度限制。</param>
    private static void ImageCompressionProcess(string filePath, string outputDirectory, int maxWidth)
    {
        var fileName = Path.GetFileName(filePath);
        var outputFilePath = Path.Combine(outputDirectory, fileName);
        try
        {
            ApplyLosslessCompression(filePath, outputFilePath, maxWidth);

            LogCompressionResults(filePath, outputFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"處理檔案 {filePath} 時發生錯誤: {ex.Message}");
        }
    }

    /// <summary>
    /// 紀錄壓縮結果，顯示原始檔案與壓縮檔案的大小，並計算檔案大小減少量及百分比。
    /// </summary>
    /// <param name="filePath">壓縮前原始檔案的路徑。</param>
    /// <param name="outputFilePath">壓縮後檔案的儲存路徑。</param>
    private static void LogCompressionResults(string filePath, string outputFilePath)
    {
        // 原始檔案大小
        var originalSize = new FileInfo(filePath).Length;
        // 壓縮後檔案大小
        var compressedSize = new FileInfo(outputFilePath).Length;

        // 計算壓縮後與壓縮前的差異
        var sizeDifference = originalSize - compressedSize;
        var sizeDifferencePercentage = ((double)sizeDifference / originalSize) * 100;

        // 印出檔案大小變化
        Console.WriteLine($"處理完成：{filePath}");
        Console.WriteLine($"原始大小：{originalSize / 1024.0:F2} KB");
        Console.WriteLine($"壓縮後大小：{compressedSize / 1024.0:F2} KB");
        Console.WriteLine($"大小減少：{sizeDifference / 1024.0:F2} KB ({sizeDifferencePercentage:F2}%)\n");
    }

    /// <summary>
    /// 對影像檔案進行無失真壓縮，並將壓縮後的結果儲存至指定的輸出路徑。
    /// </summary>
    /// <param name="filePath">需要壓縮的影像檔案路徑。</param>
    /// <param name="outputFilePath">壓縮後影像的儲存路徑。</param>
    /// <param name="maxWidth">影像壓縮後的最大寬度。如果影像寬度超過此值，將按照比例調整大小。</param>
    private static void ApplyLosslessCompression(string filePath, string outputFilePath, int maxWidth)
    {
        var fileExtension = Path.GetExtension(filePath).ToLower();
        switch (fileExtension)
        {
            case ".jpg":
                ResizeImage(filePath, outputFilePath, maxWidth, JpegEncoder);
                return;
            case ".png":
                ResizeImage(filePath, outputFilePath, maxWidth, PngEncoder);
                return;
            case ".webp":
                ResizeImage(filePath, outputFilePath, maxWidth, WebpEncoder);
                return;
            default:
                throw new NotSupportedException($"檔案格式 {fileExtension} 不受支持");
        }
    }

    /// <summary>
    /// 調整圖片大小以適應指定的最大寬度，同時保持原始比例，
    /// 並使用指定的編碼器將調整後的圖片儲存到輸出檔案路徑。
    /// </summary>
    /// <param name="inputFilePath">要調整大小的來源圖片文件的檔案路徑。</param>
    /// <param name="outputFilePath">儲存調整後圖片的目標檔案路徑。</param>
    /// <param name="maxWidth">調整後圖片的最大寬度。</param>
    /// <param name="encoder">用於儲存調整後圖片的影像編碼器。</param>
    private static void ResizeImage(string inputFilePath, string outputFilePath, int maxWidth,
        ImageEncoder encoder)
    {
        using var image = Image.Load(inputFilePath);
        if (image.IsSizeAcceptable(maxWidth))
        {
            // 直接複製圖片到輸出路徑
            image.Save(outputFilePath, encoder);
            return;
        }

        // 計算新的寬高比，維持原始比例
        var resizeOptions = new ResizeOptions
        {
            Size = new Size(maxWidth, 0),
            Mode = ResizeMode.Max // 確保圖片寬/高同時限制在最大值內，比例不變
        };
        image.Mutate(x => x.Resize(resizeOptions));

        image.Save(outputFilePath, encoder);
    }
}

public static class ImageExtensions
{
    /// <summary>
    /// 判斷圖片的寬度是否在允許的範圍內。
    /// </summary>
    /// <param name="image">要檢查的圖片實例。</param>
    /// <param name="width">允許的最大寬度。</param>
    /// <returns>
    /// 如果圖片的寬度小於或等於指定的最大寬度，則返回 true；否則返回 false。
    /// </returns>
    public static bool IsSizeAcceptable(this Image image, int width)
    {
        return image.Width <= width;
    }
}