# 圖片處理工具 (Image Processing Tool)

## 描述
這是一個用於處理圖像的簡單工具程式，包括：
- 檢查圖片大小是否符合設定條件。
- 如果圖片寬度超過 1320px，自動將其按比例縮小至 1320px。
- 圖片壓縮功能（支援無損壓縮）。
- 在每次執行時打印程式的執行路徑。

> 此工具支援 Windows、macOS 和 Linux 平台，適用於 .NET 8.0。

---

## 功能特色
1. **圖片尺寸調整**：
   如圖片寬度大於 1320px，會自動按比例縮放至 1320px，高度不受限制。

2. **支援無損壓縮**：
   圖片將進行壓縮後輸出，同時保留較佳的圖片品質。

3. **紀錄功能**：
   處理過的檔案會自動紀錄，避免重複處理相同圖片。

4. **執行路徑記錄**：
   程式啟動時，自動在 Console 輸出目前應用程式的執行路徑，便於檢查執行環境。

---

## 系統需求
- **.NET 8.0** 或以上版本
- 平台支援：Windows、macOS、Linux

---

## 使用方式

1. 編譯並執行此工具。
2. 執行命令時提供參數：
   ```bash
   dotnet run -s <SourceDirectory> -o <OutputDirectory> -c <Count> -e <FileSizeLimit>
   ```
    - `<SourceDirectory>`：來源圖片目錄。
    - `<OutputDirectory>`：處理後的圖片輸出目錄。
    - `<Count>`：處理的圖片數量，例如設置 `10` 表示最多處理 10 個檔案。
    - `<FileSizeLimit>`：圖片大小限制（以 MB 為單位），例如設置 `5` 表示處理 5MB 以下的檔案。

### 範例執行
以下命令將對指定來源資料夾中的圖片進行處理：
```bash
dotnet run -s "./input" -o "./output" -c 100 -e 5
```

### 預期結果
- 檔案大小超過 5MB 且寬度超過 1320px 的圖片會被縮小，並進行無損壓縮。
- 所有處理後的圖片將儲存於 `./output` 資料夾中。
- 處理過的檔案名稱會自動記錄，避免重複處理。

---

## 開發與測試
此工具使用了以下技術：
- **C# 12.0**
- **SixLabors.ImageSharp** (處理圖片縮放與壓縮)

---

## 注意事項
- 輸出的圖片格式目前僅支援 JPEG 格式。
- 項目開發時需要預先安裝 `SixLabors.ImageSharp` 套件：
   ```bash
   dotnet add package SixLabors.ImageSharp
   ```

---

## 常見問題
1. **圖片處理後沒有改變？**
    - 如果圖片的寬度已經小於或等於 1320px，程式將不會對其進行縮放。

2. **如何變更圖片壓縮品質？**
    - 在壓縮邏輯中，預設的 JPEG 壓縮品質為 `75`，可在 `ResizeImage` 方法中進行修改。

---

## Usage

### Command Line Arguments

- `-s, --source` (required): The source directory containing the images to be compressed.
- `-o, --output` (required): The output directory where the compressed images will be saved.
- `-c, --count` (optional): The number of images to process. Default is 100.
- `-e, --size` (optional): The number of image file size limit in MB. Default is 5.
- 
### Example

```sh
dotnet run -- -s "path/to/source" -o "path/to/output" -c 50