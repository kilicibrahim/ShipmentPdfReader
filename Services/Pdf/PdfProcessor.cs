using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public List<PageData> ProcessPdf()
        {
            Dictionary<int, string> pdfContentPages = _pdfReader.ReadContent();
            List<PageData> extractedPagesData = _dataExtractor.ExtractData(pdfContentPages);
            List<PageData> processedPagesData = _dataProcessor.ProcessData(extractedPagesData);

            return processedPagesData;
        }
    }

}
//TODO: buttons on the left side of the screen will go to settings tab
//TODO: a robust implemented list view