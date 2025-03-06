
## Requirements

- .NET 6.0 or later
- [CommandLineParser](https://www.nuget.org/packages/CommandLineParser/)
- [ImageSharp](https://www.nuget.org/packages/SixLabors.ImageSharp/)

## Usage

### Command Line Arguments

- `-s, --source` (required): The source directory containing the images to be compressed.
- `-o, --output` (required): The output directory where the compressed images will be saved.
- `-c, --count` (optional): The number of images to process. Default is 100.

### Example

```sh
dotnet run -- -s "path/to/source" -o "path/to/output" -c 50