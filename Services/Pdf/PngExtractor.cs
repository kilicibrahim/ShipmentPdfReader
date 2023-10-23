using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ShipmentPdfReader.Services.Pdf
{
    public class PngExtractor
    {
        private readonly ConfigurationManager _configManager;
        private int _currentDirectoryNumber = 1;
        public const int ItemNumberInPage = 25;
        public PngExtractor()
        {
            _configManager = ConfigurationManager.Instance;
        }

        public void CreatePngs(ObservableCollection<ExtractedData> data)
        {
            try
            {
                int runningTotal;
                foreach (var dataItem in data)
                {
                    var directoryName = _currentDirectoryNumber.ToString();
                    var directoryPath = System.IO.Path.Combine(_configManager.DestinationDirectoryPath, directoryName);
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    ExecuteCreatePng(dataItem);
                    var fileCount = Directory.GetFiles(directoryPath).Length;
                    runningTotal = fileCount;
                    if (runningTotal > ItemNumberInPage)
                    {
                        _currentDirectoryNumber++;
                    }
                }
            }
            catch(Exception ex) 
            {
                Trace.WriteLine($"wtf: {ex}");
            }

        }
        private void ExecuteCreatePng(ExtractedData extractedData)
        {
            var pageNumber = extractedData.PageNumber;

            foreach (var dataItems in extractedData.Items)
            {
                var sku = dataItems.Sku;
                for (var i = 0; i < dataItems.Quantity; i++)
                {
                    foreach (var item in dataItems.ProcessedItems)
                    {
                        CreatePngFile(item, pageNumber, sku);
                    }
                }
            }
        }

        private void CreatePngFile(ProcessedItem item, int pageNumber, string skuCode)
        {
            var sizeValue = item.SizeValue;
            var fontColor = item.FontColor?.Trim() ?? string.Empty;

            var descriptorSuffix = item.Descriptor.Contains('_')
                ? item.Descriptor.Split('_')[1].ToUpperInvariant()
                : item.Descriptor;

            var searchPattern = $"{skuCode}-*-{fontColor}.png";
            var pngFiles = Directory.GetFiles(_configManager.SourceDirectoryPath, searchPattern)
                                    .Where(file => file.ToUpperInvariant().Contains($"-{descriptorSuffix.ToUpperInvariant()}-"));

            foreach (var file in pngFiles)
            {
                var destinationPath = GetDestinationPath(pageNumber, item.Descriptor, sizeValue, fontColor);
                if (!string.IsNullOrEmpty(destinationPath))
                {
                    File.Copy(file, destinationPath);
                }
            }
        }

        private string GetDestinationPath(int pageNumber, string descriptor, float sizeValue, string fontColor)
        {
            var directoryName = _currentDirectoryNumber.ToString();
            var directoryPath = System.IO.Path.Combine(_configManager.DestinationDirectoryPath, directoryName);

            var destinationFileName = $"{pageNumber}-{descriptor}-{sizeValue}-{fontColor}.png";
            var iteration = 0;
            var destinationPath = System.IO.Path.Combine(directoryPath, destinationFileName);

            while (File.Exists(destinationPath))
            {
                iteration++;
                destinationFileName = $"{pageNumber}-{descriptor}-{sizeValue}-{fontColor}{iteration}.png";
                destinationPath = System.IO.Path.Combine(directoryPath, destinationFileName);
            }

            return destinationPath;
        }

    }
}
