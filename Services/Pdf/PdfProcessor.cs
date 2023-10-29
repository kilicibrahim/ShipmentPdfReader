using CommunityToolkit.Mvvm.Messaging;
using ShipmentPdfReader.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;

namespace ShipmentPdfReader.Services.Pdf
{
    public class PdfProcessor
    {
        private readonly PdfRead _pdfReader;
        private readonly DataExtractor _dataExtractor;
        private readonly DataProcessor _dataProcessor;

        public PdfProcessor(string filePath)
        {
            _pdfReader = new PdfRead(filePath);
            _dataExtractor = new DataExtractor();
            _dataProcessor = new DataProcessor();
        }

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
                        }
                    }
                    processedPagesData = _dataProcessor.ProcessData(extractedPagesData);
                    var summaryInfo = new SummaryInfo();
                    summaryInfo.UpdateFromPageData(processedPagesData); //TODO: Implement the output of summaryInfo
                }
                catch (Exception ex)
                {
                    WeakReferenceMessenger.Default.Send(new Messages(ex.ToString()));
                }
                return processedPagesData;
            });
        }
        private List<PageData> SortPages(List<PageData> pages, string firstCriterion, string secondCriterion, string thirdCriterion)
        {
            var sortString = BuildSortString(firstCriterion, secondCriterion, thirdCriterion);

            var singleItemPages = pages.Where(p => p.Extracted.Items.Count == 1).ToList();
            var multipleItemsPages = pages.Where(p => p.Extracted.Items.Count != 1).ToList();

            singleItemPages = singleItemPages
                .AsQueryable()
                .OrderBy(sortString)
                .ToList();

            singleItemPages.AddRange(multipleItemsPages);

            return singleItemPages;
        }
        private string BuildSortString(string firstCriterion, string secondCriterion, string thirdCriterion)
        {
            var criteria = new List<string>();
            if (!string.IsNullOrEmpty(firstCriterion))
                criteria.Add($"Extracted.Items[0].{firstCriterion}");
            if (!string.IsNullOrEmpty(secondCriterion))
                criteria.Add($"Extracted.Items[0].{secondCriterion}");
            if (!string.IsNullOrEmpty(thirdCriterion))
                criteria.Add($"Extracted.Items[0].{thirdCriterion}");

            return string.Join(", ", criteria);
        }

    }

}