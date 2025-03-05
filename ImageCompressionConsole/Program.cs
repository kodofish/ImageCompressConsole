using System;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

class Program
{
    static void Main(string[] args)
    {
        // 確保傳入了來源和輸出目錄
        if (args.Length < 2)
        {
            Console.WriteLine("用法: <程式名稱> <來源目錄> <輸出目錄>");
            return;
        }
        
        // 由參數取得來源目錄和輸出目錄
        string sourceDirectory = args[0];
        string outputDirectory = args[1];
        
        // 如果輸出目錄不存在，創建它
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        // 取得目錄中所有 jpg 和 png 圖檔
        string[] imageFiles = Directory.GetFiles(sourceDirectory, "*.*")
            .Where(file => file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                           file.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
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
            // 讀取圖檔
            using (Image image = Image.Load(filePath))
            {
                // 進行無損壓縮（此處僅示意調整參數，實際操作需根據需求測試調整）
                string fileExtension = Path.GetExtension(filePath).ToLower();
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string outputFilePath = Path.Combine(outputDirectory, fileName + fileExtension);

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
            }

            Console.WriteLine($"壓縮完成：{filePath} -> {outputDirectory}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"處理檔案 {filePath} 時發生錯誤: {ex.Message}");
        }
    }
}