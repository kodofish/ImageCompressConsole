using CommandLine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

namespace ImageCompressionConsole;

// 定義命令列選項的類別
public class DirectoryOptions
{
    [Option('s', "source", Required = true, HelpText = "來源目錄，包含要壓縮的圖片檔案")]
    public string SourceDirectory { get; set; }

    [Option('o', "output", Required = true, HelpText = "輸出目錄，壓縮後的圖片存放位置")]
    public string OutputDirectory { get; set; }
}

public class HelpOptions
{
    [Option('h', "help", Required = false)]
    public bool Help { get; set; } = true;
}

static class Program
{
    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<DirectoryOptions>(args)
            .WithParsed(options => CompressImages(options))
            .WithNotParsed(errors =>
            {
                Console.WriteLine("Parameter parsing failed, please check if the command format is correct.\n");
                Environment.Exit(1);
            });
    }

    private static void CompressImages(DirectoryOptions directoryOptions)
    {
        // 由參數取得來源目錄和輸出目錄
        string sourceDirectory = directoryOptions.SourceDirectory;
        string outputDirectory = directoryOptions.OutputDirectory;

        // 如果輸出目錄不存在，創建它
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        // 取得目錄中所有 jpg 和 png 圖檔
        var imageFiles = Directory.GetFiles(sourceDirectory, "*.*")
            .Where(file => file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                           file.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(file => new FileInfo(file).Length)
            .Take(10)
            .ToArray();

        foreach (var filePath in imageFiles)
        {
            ImageCompressionProcess(filePath, outputDirectory);
        }

        Console.WriteLine("所有圖檔已完成處理");
    }

    private static void ImageCompressionProcess(string filePath, string outputDirectory)
    {
        try
        {
            // 原始檔案大小
            var originalSize = new FileInfo(filePath).Length;

            // 讀取圖檔
            using (var image = Image.Load(filePath))
            {
                // 進行無損壓縮（此處僅示意調整參數，實際操作需根據需求測試調整）
                var fileExtension = Path.GetExtension(filePath).ToLower();
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var outputFilePath = Path.Combine(outputDirectory, fileName + fileExtension);

                if (fileExtension == ".jpg")
                {
                    // JPEG 無損壓縮
                    var jpegEncoder = new JpegEncoder()
                    {
                        Quality = 100 // 設定高品質以實現無損壓縮
                    };
                    image.Save(outputFilePath, jpegEncoder);
                }
                else if (fileExtension == ".png")
                {
                    // PNG 壓縮
                    var pngEncoder = new PngEncoder()
                    {
                        CompressionLevel = PngCompressionLevel.BestCompression // 無損壓縮
                    };
                    image.Save(outputFilePath, pngEncoder);
                }
                
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

            Console.WriteLine($"壓縮完成：{filePath} -> {outputDirectory}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"處理檔案 {filePath} 時發生錯誤: {ex.Message}");
        }
    }
}