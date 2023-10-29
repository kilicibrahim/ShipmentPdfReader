using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    if (!string.IsNullOrWhiteSpace(item.Sku))
                    {
                        if (SkuCounts.ContainsKey(item.Sku))
                            SkuCounts[item.Sku]++;
                        else
                            SkuCounts[item.Sku] = 1;
                    }

                    if (!string.IsNullOrWhiteSpace(item.Size))
                    {
                        if (SizeCounts.ContainsKey(item.Size))
                            SizeCounts[item.Size]++;
                        else
                            SizeCounts[item.Size] = 1;
                    }

                    if (!string.IsNullOrWhiteSpace(item.Color))
                    {
                        if (ColorCounts.ContainsKey(item.Color))
                            ColorCounts[item.Color]++;
                        else
                            ColorCounts[item.Color] = 1;
                    }
                    string sizeColorKey = $"{item.Size ?? "UnknownSize"}-{item.Color ?? "UnknownColor"}";
                    if (SizeColorCombinationCounts.ContainsKey(sizeColorKey))
                        SizeColorCombinationCounts[sizeColorKey]++;
                    else
                        SizeColorCombinationCounts[sizeColorKey] = 1;
                }
            }
        }
    }

}
