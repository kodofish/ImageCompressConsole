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
- **.NET 8 SDK** 或以上版本
- 支援的作業系統：
   - Windows
   - macOS
   - Linux

如尚未安裝 .NET 8 SDK，請參考下方的安裝步驟。

---
## 安裝 .NET 8 SDK

若系統未預先安裝 **.NET 8 SDK**，請依照以下指引操作：

- **Linux**:
   1. 安裝相依套件：
      ```bash
      sudo apt update
      sudo apt install -y lsb-release
      sudo apt install -y wget apt-transport-https ca-certificates
      ```
   2. 執行以下指令來新增 Microsoft 的官方 APT 儲存庫：
      ```bash
      wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
      sudo dpkg -i packages-microsoft-prod.deb
      sudo apt update
      ```
   3. 安裝 .NET 8 SDK：
      ```bash
      sudo apt install -y dotnet-sdk-8.0
      ``` 
   4. 若想確認安裝是否成功，可以執行：
      ```bash
      dotnet --list-skds
      ```
      上述命令將顯示已安裝的運行時清單。確定列出了版本 `8.x.x` 的 ASP.NET Core Runtime。
   5. 配置路徑（如果安裝後無法執行 `dotnet` 指令）：
      ```bash
     export PATH="$HOME/.dotnet/tools:$HOME/.dotnet:$PATH"
      ```
- **macOS**:
   1. 使用 Homebrew 安裝 .NET Runtime：
      ```bash
      brew install --cask dotnet
      ```
   2. 配置路徑（如果安裝後無法執行 `dotnet` 指令）：
      ```bash
      export PATH="$PATH:/usr/local/share/dotnet"
      ```

- **Windows**:
   1. [點擊此處下載 .NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0/runtime)。
   2. 下載後執行安裝程式，並按照安裝指示完成安裝。

#### 驗證 .NET 安裝
安裝完成後可以執行以下命令，檢查系統上是否已成功安裝 .NET：
```bash
dotnet --version
```
若顯示的版本是 `8.x.x` 或以上，即表示安裝成功。

---

## 安裝方式

此程式已釋出為 .NET 全球工具，可通過 NuGet 平台進行安裝。

### 1. 安裝 .NET 全球工具
使用以下命令將此工具安裝到系統中：
```bash
dotnet tool install --global ImageCompressionConsole
```

### 2. 更新工具版本
如果已安裝此工具，並且希望更新至最新版本，請執行以下命令：
```bash
dotnet tool update -g ImageCompressionConsole
```

### 3. 卸載工具
如需從系統中移除此工具，可使用以下命令：
```bash
dotnet tool uninstall -g ImageCompressionConsole
```

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
- 檔案大小超過 5MB 且寬度超過 1320px 的圖片會被縮小，並進行壓縮。
- 所有處理後的圖片將儲存於 `./output` 資料夾中。
- 處理過的檔案名稱會自動記錄，避免重複處理。

---

## 開發與測試
此工具使用了以下技術：
- **C# 12.0**
- **SixLabors.ImageSharp** (處理圖片縮放與壓縮)

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