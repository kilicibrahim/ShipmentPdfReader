using CommunityToolkit.Mvvm.Messaging;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using ShipmentPdfReader.Helpers;
using System.Data;
using System.Linq.Dynamic.Core;

namespace ShipmentPdfReader.Services.Pdf
{
    public class PdfProcessor
    {
        private readonly PdfRead _pdfReader;
        private readonly DataExtractor _dataExtractor;
        private readonly DataProcessor _dataProcessor;
        private readonly ConfigurationManager _configManager;
        private readonly string _pdfFilePath;
        private readonly string _pdfFileName;
        public PdfProcessor(string filePath)
        {
            _pdfReader = new PdfRead(filePath);
            _pdfFileName = Path.GetFileName(filePath);
            _pdfFilePath = filePath;
            _dataExtractor = new DataExtractor();
            _dataProcessor = new DataProcessor();
            _configManager = ConfigurationManager.Instance;
        }

        /// <summary>
        /// Processes the PDF asynchronously, including reading, extracting data, sorting (if enabled),
        /// and reordering pages.
        /// </summary>
        /// <returns>A list of processed PageData.</returns>
        public async Task<List<PageData>> ProcessPdfAsync()
        {
            return await Task.Run(() =>
            {
                List<PageData> processedPagesData = null;
                try
                {
                    bool isSortingEnabled = Preferences.Get("IsSortingEnabled", false);

                    Dictionary<int, string> pdfContentPages = _pdfReader.ReadContent();
                    List<PageData> extractedPagesData = _dataExtractor.ExtractData(pdfContentPages);
                    if (extractedPagesData == null || !extractedPagesData.Any())
                    {
                        throw new InvalidOperationException("No data was extracted from the PDF.");
                    }
                    if (isSortingEnabled)
                    {
                        string firstCriterion = Preferences.Get("FirstSortCriterion", "");
                        string secondCriterion = Preferences.Get("SecondSortCriterion", "");
                        string thirdCriterion = Preferences.Get("ThirdSortCriterion", "");

                        if (!string.IsNullOrEmpty(firstCriterion) ||
                            !string.IsNullOrEmpty(secondCriterion) ||
                            !string.IsNullOrEmpty(thirdCriterion))
                        {
                            extractedPagesData = SortPages(extractedPagesData, firstCriterion, secondCriterion, thirdCriterion);
                            List<int> newOrder = extractedPagesData.Select(p => p.PageNumber).ToList();

                            string baseOutputPdfName = "SortedOutput";
                            string outputPdfPath = FileHelper.GenerateUniqueFilename(_configManager.DestinationDirectoryPath, baseOutputPdfName, ".pdf");
                            ReorderPdf(newOrder, outputPdfPath);
                            if (extractedPagesData != null && extractedPagesData.Any())
                            {
                                int newPageIndex = 1;
                                foreach (var pageData in extractedPagesData)
                                {
                                    pageData.PageNumber = newPageIndex;
                                    if (pageData.Extracted != null)
                                    {
                                        pageData.Extracted.PageNumber = newPageIndex;
                                    }
                                    newPageIndex++;
                                }
                            }
                        }
                    }
                    processedPagesData = _dataProcessor.ProcessData(extractedPagesData);
                    var summaryInfo = new SummaryInfo();
                    summaryInfo.UpdateFromPageData(processedPagesData); 
                    DataTable dataTable = summaryInfo.PrepareDataForExcel(summaryInfo.SizeColorCombinationCounts);
                    var summaryXslxFilePath = _configManager.DestinationDirectoryPath + "\\" + _pdfFileName + ".xlsx";
                    Microsoft.Maui.Controls.Application.Current.Dispatcher.Dispatch(async () =>
                    {
                        await summaryInfo.ExportToExcelAsync(dataTable, summaryXslxFilePath);
                    });

                }
                catch (Exception ex)
                {
                    WeakReferenceMessenger.Default.Send(new Messages(ex.ToString()));
                }
                return processedPagesData;
            });
        }

        /// <summary>
        /// Reorders the pages of a PDF document based on the specified order and outputs to a new PDF file.
        /// </summary>
        /// <param name="pageOrder">List of integers representing the new page order.</param>
        /// <param name="outputPdfPath">Output path for the reordered PDF file.</param>
        public void ReorderPdf(List<int> pageOrder, string outputPdfPath)
        {
            string originalPdfPath = _pdfFilePath;
            using (PdfReader reader = new PdfReader(originalPdfPath))
            {
                using (FileStream fs = new FileStream(outputPdfPath, FileMode.Create, FileAccess.Write))
                {
                    Document document = new Document(reader.GetPageSizeWithRotation(1));
                    PdfWriter writer = PdfWriter.GetInstance(document, fs);
                    document.Open();

                    PdfContentByte cb = writer.DirectContent;
                    foreach (int pageNumber in pageOrder)
                    {
                        document.NewPage();
                        PdfImportedPage page = writer.GetImportedPage(reader, pageNumber);
                        cb.AddTemplate(page, 0, 0);
                    }

                    document.Close();
                }
            }
        }

        /// <summary>
        /// Sorts the given pages based on specified sorting criteria. Keeps pages with multiple items at the end.
        /// </summary>
        /// <param name="pages">The list of PageData to sort.</param>
        /// <param name="firstCriterion">The primary sorting criterion.</param>
        /// <param name="secondCriterion">The secondary sorting criterion.</param>
        /// <param name="thirdCriterion">The tertiary sorting criterion.</param>
        /// <returns>The sorted list of PageData.</returns>
        private List<PageData> SortPages(List<PageData> pages, string firstCriterion, string secondCriterion, string thirdCriterion)
        {
            var sortCriteria = GetSortCriteria(firstCriterion, secondCriterion, thirdCriterion);
            if (!sortCriteria.Any())
            {
                return pages;
            }
            var singleItemPages = new List<PageData>();
            var multipleItemsPages = new List<PageData>();

            foreach (var page in pages)
            {
                if (page.Extracted.Items.Count > 0 && !string.IsNullOrEmpty(firstCriterion) &&
                    page.Extracted.Items.TrueForAll(item =>
                    {
                        var itemProperty = item.GetType().GetProperty(firstCriterion);
                        if (itemProperty == null)
                            return false;

                        var firstItemProperty = page.Extracted.Items[0].GetType().GetProperty(firstCriterion);
                        if (firstItemProperty == null)
                            return false;

                        var itemValue = itemProperty.GetValue(item, null);
                        var firstItemValue = firstItemProperty.GetValue(page.Extracted.Items[0], null);

                        if (itemValue == null && firstItemValue == null)
                            return true; 

                        return itemValue?.ToString() == firstItemValue?.ToString();
                    }))
                {
                    singleItemPages.Add(page);
                }
                else
                {
                    multipleItemsPages.Add(page);
                }
            }

            var sortString = string.Join(", ", sortCriteria);

            singleItemPages = singleItemPages
                .AsQueryable()
                .OrderBy(sortString)
                .ToList();

            singleItemPages.AddRange(multipleItemsPages);

            return singleItemPages;
        }


        /// <summary>
        /// Retrieves the sorting criteria for dynamic LINQ queries based on provided sorting criteria.
        /// </summary>
        /// <param name="firstCriterion">Primary sorting criterion.</param>
        /// <param name="secondCriterion">Secondary sorting criterion.</param>
        /// <param name="thirdCriterion">Tertiary sorting criterion.</param>
        /// <returns>A string representing the sorting criteria for dynamic LINQ query.</returns>
        private IEnumerable<string> GetSortCriteria(params string[] criteria)
        {
            return criteria.Where(c => !string.IsNullOrEmpty(c)).Select(c => $"Extracted.Items[0].{c}");
        }
        public class ColorPair
        {

            [JsonProperty("Color")]
            public string Color
            {
                get; set;
            }
        }

        public async Task DividePdfByUserSelectionAsync(string jsonFilePath)
        {
            string jsonContent = File.ReadAllText(jsonFilePath);
            var selectedCombinations = JsonConvert.DeserializeObject<List<ColorSizePair>>(jsonContent);

            var pdfContentPages = _pdfReader.ReadContent();
            var extractedPagesData = _dataExtractor.ExtractData(pdfContentPages);

            var normalizedSelectedCombinations = selectedCombinations
                .Select(c => new { Color = c.Color.ToLowerInvariant(), Size = c.Size.ToLowerInvariant() })
                .ToList();

            var filteredPages = extractedPagesData
                .Where(p => p?.Extracted?.Items != null && p.Extracted.Items.Count > 0)
                .Where(p => p.Extracted.Items.Exists(item =>
                    normalizedSelectedCombinations.Exists(nc =>
                        nc.Color == item?.Color?.ToLowerInvariant() &&
                        nc.Size == item?.Size?.ToLowerInvariant())))
                .ToList();

            var groupedPages = filteredPages
                .GroupBy(p => new { p.Extracted.Items[0].Size, p.Extracted.Items[0].Color })
                .ToDictionary(g => g.Key, g => g.Select(p => p.PageNumber).ToList());

            var allCombinedPageNumbers = groupedPages.SelectMany(g => g.Value).Distinct().ToList();

            string allCombinationsPdfName = "AllCombinations.pdf";
            string allCombinationsPdfPath = FileHelper.GenerateUniqueFilename(_configManager.DestinationDirectoryPath, allCombinationsPdfName, ".pdf");

            CreatePdfFromPages(allCombinedPageNumbers, allCombinationsPdfPath);

            var allPageNumbers = extractedPagesData.Select(p => p.PageNumber).ToHashSet();
            var combinedPages = groupedPages.SelectMany(g => g.Value).ToHashSet();
            var remainingPageNumbers = allPageNumbers.Except(combinedPages).ToList();

            if (remainingPageNumbers.Any())
            {
                string outputPdfName = "RemainingPages.pdf";
                string outputPdfPath = FileHelper.GenerateUniqueFilename(_configManager.DestinationDirectoryPath, outputPdfName, ".pdf");

                CreatePdfFromPages(remainingPageNumbers, outputPdfPath);
            }

            WeakReferenceMessenger.Default.Send(new Messages("PDFs divided based on user selection successfully."));
        }
        public class ColorSizePair
        {
            public string Color
            {
                get; set;
            }
            public string Size
            {
                get; set;
            }
        }

        public void CreatePdfFromPages(List<int> pageOrder, string outputPdfPath)
        {
            string originalPdfPath = _pdfFilePath;
            using (PdfReader reader = new PdfReader(originalPdfPath))
            {
                using (FileStream fs = new FileStream(outputPdfPath, FileMode.Create, FileAccess.Write))
                {
                    Document document = new Document(reader.GetPageSizeWithRotation(1));
                    PdfWriter writer = PdfWriter.GetInstance(document, fs);
                    document.Open();

                    PdfContentByte cb = writer.DirectContent;
                    foreach (int pageNumber in pageOrder)
                    {
                        document.NewPage();
                        PdfImportedPage page = writer.GetImportedPage(reader, pageNumber);
                        cb.AddTemplate(page, 0, 0);
                    }

                    document.Close();
                }
            }
        }

        public static async Task ExtractExcelsFromPageData(List<PageData> processedPagesData, string path)
        {
            var summaryInfo = new SummaryInfo();
            summaryInfo.UpdateFromPageData(processedPagesData);
            DataTable dataTable = summaryInfo.PrepareDataForExcel(summaryInfo.SizeColorCombinationCounts);
            Application.Current.Dispatcher.Dispatch(async () =>
            {
                await summaryInfo.ExportToExcelAsync(dataTable, path);
            });
        }
    }

}