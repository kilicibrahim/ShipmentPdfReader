
using ShipmentPdfReader.Models;
using System.Linq;

namespace ShipmentPdfReader.Services.Pdf
{
    public class ExtractedData
    {
        public int TotalItems { get; set; }
        public int PageNumber { get; set; }
        public bool IsPersonalized { get; set; }
        public ExtractedData()
        {
            Items = new List<Item>();
        }
      
        public static void ConvertToItems(ItemData itemData, List<Item> items, bool isSkuNeeded = true)
        {
            if (isSkuNeeded)
            {
                int count = itemData.SkuCodes.Count;

                for (int i = 0; i < count; i++)
                {
                    if (string.IsNullOrWhiteSpace(itemData.SkuCodes[i]))
                    {
                        Console.WriteLine($"Warning: Missing SKU at position {i}. Skipping item.");
                        continue;
                    }

                    items.Add(new Item
                    {
                        Quantity = i < itemData.Quantities.Count ? itemData.Quantities[i] : (int?)null,
                        Sku = itemData.SkuCodes[i],
                        Size = i < itemData.Sizes.Count ? itemData.Sizes[i] : null,
                        Color = i < itemData.Colors.Count ? itemData.Colors[i] : null
                    });
                }
            }
            else
            {
                int count = Math.Max(itemData.Sizes.Count, itemData.Colors.Count);
                
                for (int i = 0; i < count; i++)
                {
                    items.Add(new Item
                    {
                        Quantity = i < itemData.Quantities.Count ? itemData.Quantities[i] : (int?)null,
                        Sku = i < itemData.SkuCodes.Count ? itemData.SkuCodes[i] : null,
                        Size = i < itemData.Sizes.Count ? itemData.Sizes[i] : null,
                        Color = i < itemData.Colors.Count ? itemData.Colors[i] : null
                    });
                }
            }
        }

        public List<Item> Items { get; set; }
        public List<Item> ItemsForExcel { get; set; } = new List<Item>();
    }

    public class ProcessedData
    {
        public List<string> LogMessages { get; set; } = new List<string>();
        public List<string> WarningMessages { get; set; } = new List<string>();
    }

    public class ProcessedItem
    {
        public string Descriptor
        {
            get; set; 
        }
        public string FontColor
        {
            get; set;
        }
        public float SizeValue
        {
            get; set;
        }
    }

    public class PageData
    {
        private ExtractedData _extracted;

        public int PageNumber { get; set; }

        public ExtractedData Extracted
        {
            get => _extracted;
            set
            {
                _extracted = value;
                if (_extracted != null)
                {
                    PageNumber = _extracted.PageNumber;
                }
            }
        }
        public ProcessedData Processed { get; set; }
    }
    public class ItemData
    {
        public List<string> SkuCodes { get; set; } = new List<string>();
        public List<string> Sizes { get; set; } = new List<string>();
        public List<string> Colors { get; set; } = new List<string>();
        public List<int> Quantities { get; set; } = new List<int>();
    }
    public class ContentData
    {
        public List<string> SkuCodes { get; set; } = new List<string>();
        public List<string> Sizes { get; set; } = new List<string>();
        public List<string> Colors { get; set; } = new List<string>();
        public List<int> Quantities { get; set; } = new List<int>();
    }

    public class Item : ObservableObject
    {
        public int? Quantity { get; set; }
        public string Sku { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public List<ProcessedItem> ProcessedItems { get; set; } = new List<ProcessedItem>();

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }
    }

}
