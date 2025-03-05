using CommandLine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

namespace ImageCompressionConsole;

public class Options
{
    [Option('s', "source", Required = true, Default = null, HelpText = "來源目錄，包含要壓縮的圖片檔案")]
    public string SourceDirectory { get; set; }

    [Option('o', "output", Required = true, Default = null, HelpText = "輸出目錄，壓縮後的圖片存放位置")]
    public string OutputDirectory { get; set; }

    [Option('c', "count", Required = false, Default = 100, HelpText = "要處理的圖片數量, 預設為 100")]
    public int Count { get; set; }
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
        // 如果紀錄檔不存在，返回空集合
        if (!File.Exists(recordFilePath))
        {
            return new HashSet<string>();
        }

        // 將紀錄檔中的檔案路徑讀取為集合
        return new HashSet<string>(File.ReadAllLines(recordFilePath));
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
        HashSet<string> processedFiles = LoadProcessedFiles(recordFilePath);


        // 取得來源目錄下的所有圖檔，並按檔案大小排序，取前 10 個進行壓縮
        var imageFiles = Directory.GetFiles(sourceDirectory, "*.*")
            .Where(file => (file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                            file.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) &&
                           !processedFiles.Contains(file))
            .OrderByDescending(file => new FileInfo(file).Length)
            .Take(maxFilesToProcess);

        foreach (var filePath in imageFiles)
        {
            ImageCompressionProcess(filePath, outputDirectory);
            SaveProcessedFile(recordFilePath, filePath);
        }

        Console.WriteLine("所有圖檔已完成處理");
    }

    private static void ImageCompressionProcess(string filePath, string outputDirectory)
    {
        try
        {
            var fileExtension = Path.GetExtension(filePath).ToLower();
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var outputFilePath = Path.Combine(outputDirectory, fileName + fileExtension);

            ApplyLosslessCompression(filePath, fileExtension, outputFilePath);

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
    /// <param name="fileExtension">The file extension of the image (e.g., .jpg, .png).</param>
    /// <param name="outputFilePath">The path where the compressed image will be saved.</param>
    private static void ApplyLosslessCompression(string filePath, string fileExtension, string outputFilePath)
    {
        // 讀取圖檔
        using var image = Image.Load(filePath);
        // 進行無損壓縮（此處僅示意調整參數，實際操作需根據需求測試調整）
        if (fileExtension == ".jpg")
        {
            image.Save(outputFilePath, JpegEncoder);
        }
        else if (fileExtension == ".png")
        {
            image.Save(outputFilePath, PngEncoder);
        }
    }
}