using CommandLine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;

namespace ImageCompressionConsole;

//todo: 1. 圖檔 size 為 1320 X 1320

public class Options
{
    [Option('s', "source", Required = true, Default = null, HelpText = "來源目錄，包含要壓縮的圖片檔案")]
    public string SourceDirectory { get; set; }

    [Option('o', "output", Required = true, Default = null, HelpText = "輸出目錄，壓縮後的圖片存放位置")]
    public string OutputDirectory { get; set; }

    [Option('c', "count", Required = false, Default = 100, HelpText = "要處理的圖片數量, 預設為 100")]
    public int Count { get; set; }
    
    // 新增檔案大小限制屬性，以 MB 為單位
    [Option('s', "size", Required = false, Default = 5, HelpText = "要處理的圖片大小限制, 預設為 5MB")]
    public double FileSizeLimit { get; set; }

}

static class Program
{
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
        // 由參數取得來源目錄和輸出目錄
        var sourceDirectory = options.SourceDirectory;
        var outputDirectory = options.OutputDirectory;
        var recordFilePath = Path.Combine(outputDirectory, "processed_files.txt"); // 紀錄檔案的路徑
        var maxFilesToProcess = options.Count;
        var fileSizeLimit = options.FileSizeLimit * 1024 * 1024;

        // 驗證來源目錄是否存在
        if (!Directory.Exists(sourceDirectory))
        {
            Console.WriteLine($"來源目錄不存在: {sourceDirectory}");
            return;
        }

        // 如果輸出目錄不存在，創建它
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        // 載入已處理檔案清單
        var processedFiles = LoadProcessedFiles(recordFilePath);

        // 取得來源目錄下的所有圖檔，並按檔案大小排序，取前 10 個進行壓縮
        var imageFiles = Directory.GetFiles(sourceDirectory, "*.*")
            .Where(file => SupportedImageExtensions.Contains(Path.GetExtension(file).ToLower()))
            .Where(file => !processedFiles.Contains(file))
            .Select(file => new { Path = file, Size = new FileInfo(file).Length })
            .Where(files => files.Size > fileSizeLimit) // 過濾檔案大小
            .OrderByDescending(file => file.Size)
            .AsEnumerable();

        Console.WriteLine($"開始處理圖檔...{imageFiles.Count()} 個圖檔，最多處理 {maxFilesToProcess} 個圖檔\n");

        imageFiles = imageFiles.Take(maxFilesToProcess);

        // 如果沒有圖檔，則不進行處理
        if (!imageFiles.Any())
        {
            Console.WriteLine("沒有圖檔需要處理");
            return;
        }

        foreach (var filePath in imageFiles.Select(file => file.Path))
        {
            ImageCompressionProcess(filePath, outputDirectory);
            SaveProcessedFile(recordFilePath, filePath);
        }

        Console.WriteLine("所有圖檔已完成處理");
    }

    private static void ImageCompressionProcess(string filePath, string outputDirectory)
    {
        var fileName = Path.GetFileName(filePath);
        var outputFilePath = Path.Combine(outputDirectory, fileName);
        try
        {
            ApplyLosslessCompression(filePath, outputFilePath);

            LogCompressionResults(filePath, outputFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"處理檔案 {filePath} 時發生錯誤: {ex.Message}");
        }
    }

    /// <summary>
    /// Logs the compression results by displaying the sizes of the original and compressed files,
    /// as well as the reduction in size and percentage decrease.
    /// </summary>
    /// <param name="filePath">The path of the original file before compression.</param>
    /// <param name="outputFilePath">The path of the compressed file after the process.</param>
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
    /// Applies lossless compression to an image file and saves the result to the specified output path.
    /// </summary>
    /// <param name="filePath">The path of the image file to be compressed.</param>
    /// <param name="outputFilePath">The path where the compressed image will be saved.</param>
    private static void ApplyLosslessCompression(string filePath, string outputFilePath)
    {
        // 讀取圖檔
        using var image = Image.Load(filePath);
        var fileExtension = Path.GetExtension(filePath).ToLower();
        switch (fileExtension)
        {
            case ".jpg":
                image.Save(outputFilePath, JpegEncoder);
                return;
            case ".png":
                image.Save(outputFilePath, PngEncoder);
                return;
            case ".webp":
                image.Save(outputFilePath, WebpEncoder);
                return;
            default:
                throw new NotSupportedException($"檔案格式 {fileExtension} 不受支持");
        }
    }
}