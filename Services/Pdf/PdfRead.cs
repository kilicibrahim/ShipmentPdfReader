using CommunityToolkit.Mvvm.Messaging;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace ShipmentPdfReader.Services.Pdf
{
    public class PdfRead
    {
        public string FilePath { get; private set; }
        public PdfRead(string filePath)
        {
            FilePath = filePath;
        }

        public Dictionary<int, string> ReadContent()
        {
            var documents = new Dictionary<int, string>();
            try
            {
                using PdfReader reader = new(FilePath);
                for (int page = 1; page <= reader.NumberOfPages; page++)
                {
                    var text = PdfTextExtractor.GetTextFromPage(reader, page, new LocationTextExtractionStrategy());
                    documents.Add(page, text);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"IO Error: {ex.Message}");
                WeakReferenceMessenger.Default.Send(new Messages("Failed to read pdf. Please contact support!"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                WeakReferenceMessenger.Default.Send(new Messages($"An unexpected error occurred: {ex.Message}"));
            }
            return documents;
        }
    }
}
