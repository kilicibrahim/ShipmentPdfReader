using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data;
using ClosedXML.Excel;


namespace ShipmentPdfReader.Services.Pdf
{
    public class SummaryInfo
    {
        public Dictionary<string, int> SkuCounts
        {
            get; set;
        }
        public Dictionary<string, int> SizeCounts
        {
            get; set;
        }
        public Dictionary<string, int> ColorCounts
        {
            get; set;
        }
        public Dictionary<string, int> SizeColorCombinationCounts
        {
            get; set;
        }

        public SummaryInfo()
        {
            SkuCounts = new Dictionary<string, int>();
            SizeCounts = new Dictionary<string, int>();
            ColorCounts = new Dictionary<string, int>();
            SizeColorCombinationCounts = new Dictionary<string, int>();

        }

        public void UpdateFromPageData(List<PageData> pages)
        {
            foreach (var page in pages)
            {
                foreach (var item in page.Extracted.Items)
                {
                    int quantity = item.Quantity ?? 1;

                    if (!string.IsNullOrWhiteSpace(item.Sku))
                    {
                        if (SkuCounts.ContainsKey(item.Sku))
                            SkuCounts[item.Sku] += quantity;
                        else
                            SkuCounts[item.Sku] = quantity;
                    }

                    if (!string.IsNullOrWhiteSpace(item.Size))
                    {
                        if (SizeCounts.ContainsKey(item.Size))
                            SizeCounts[item.Size] += quantity;
                        else
                            SizeCounts[item.Size] = quantity;
                    }

                    if (!string.IsNullOrWhiteSpace(item.Color))
                    {
                        if (ColorCounts.ContainsKey(item.Color))
                            ColorCounts[item.Color] += quantity;
                        else
                            ColorCounts[item.Color] = quantity;
                    }

                    string sizeColorKey = $"{item.Size ?? "UnknownSize"}_{item.Color ?? "UnknownColor"}";
                    if (SizeColorCombinationCounts.ContainsKey(sizeColorKey))
                        SizeColorCombinationCounts[sizeColorKey] += quantity;
                    else
                        SizeColorCombinationCounts[sizeColorKey] = quantity;
                }
            }
        }
        public DataTable PrepareDataForExcel(Dictionary<string, int> sizeColorCombinationCounts)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Size", typeof(string));
            dataTable.Columns.Add("Color", typeof(string));
            dataTable.Columns.Add("Quantity", typeof(int));

            foreach (var entry in sizeColorCombinationCounts)
            {
                var sizeColor = entry.Key.Split('_');
                var row = dataTable.NewRow();
                row["Size"] = sizeColor[0];
                row["Color"] = sizeColor[1];
                row["Quantity"] = entry.Value;
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        public async Task ExportToExcelAsync(DataTable dataTable, string filePath)
        {
            if (Path.GetExtension(filePath).ToLower() != ".xlsx")
            {
                throw new ArgumentException("File path must have a .xlsx extension");
            }
            if (File.Exists(filePath))
            {
                string directory = Path.GetDirectoryName(filePath);
                string filenameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                string extension = Path.GetExtension(filePath);
                string newFilePath = Path.Combine(directory, $"{filenameWithoutExtension}_{DateTime.Now:yyyyMMddHHmmss}{extension}");
                filePath = newFilePath;
            }

            await Task.Run(() =>
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("SizeColorData");
                    var table = worksheet.Cell(1, 1).InsertTable(dataTable.AsEnumerable());
                    table.ShowAutoFilter = true;

                    int quantityColumnIndex = dataTable.Columns.IndexOf("Quantity") + 1;

                    worksheet.Cell(1, 4).Value = "Filtered Totals";
                    worksheet.Cell(1, 4).Style.Font.Bold = true;

                    worksheet.Cell(1, 5).FormulaA1 =
                        $"=SUBTOTAL(9, {worksheet.Column(quantityColumnIndex).Cell(2).Address}:{worksheet.Column(quantityColumnIndex).LastCellUsed().Address})";
                    worksheet.Cell(1, 5).Style.Font.Bold = true;

                    workbook.SaveAs(filePath);
                }
            });
        }

    }
}
