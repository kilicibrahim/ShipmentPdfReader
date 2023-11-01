﻿using CommunityToolkit.Mvvm.Messaging;
using iTextSharp.text;
using iTextSharp.text.pdf;
using ShipmentPdfReader.Helpers;
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
        public PdfProcessor(string filePath)
        {
            _pdfReader = new PdfRead(filePath);
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
                    summaryInfo.UpdateFromPageData(processedPagesData); //TODO: Implement the output of summaryInfo
                    //TODO: Implement count 
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
    }

}