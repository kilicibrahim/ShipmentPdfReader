﻿using ShipmentPdfReader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ShipmentPdfReader.Services.Pdf
{
    internal class DataProcessor
    {
        private readonly ConfigurationManager _configManager;
        public DataProcessor()
        {
            _configManager = ConfigurationManager.Instance;
        }

        public List<PageData> ProcessData(List<PageData> pagesData)
        {
            foreach(var page in pagesData)
            {
                var processedMessage = new ProcessedData();
                page.Extracted.Items = ProcessPage(processedMessage, page);
                page.Processed = processedMessage;
            }

            return pagesData;
        }

        private List<Item> ProcessPage(ProcessedData processedMessage, PageData page)
        {
            List<Item> processedItems = new List<Item>();
            var pageNumber = page.PageNumber;

            processedMessage.LogMessages.Add($"Page {pageNumber} content:");
            processedMessage.LogMessages.Add($"Total items in page: {page.Extracted.TotalItems}");
            if(page.Extracted.IsPersonalized)
            {
                processedMessage.WarningMessages.Add($"Personalization: The file will be created in the output file. Please review manualy for client needs. On page {pageNumber}");

            }
            foreach (var item in page.Extracted.Items)
            {
                var color = item.Color;
                var skuCode = item.Sku;
                var quantity = item.Quantity;
                var size = item.Size;
                string fontColor = null;
                
                if (!string.IsNullOrEmpty(color))
                {
                    var normalizedColor = color.Replace("\u00A0", " ").Trim();
                    fontColor = _configManager.AcceptableColors.Find(cp => cp.BackgroundColor.Replace("\u00A0", " ").Trim().ToLower() == normalizedColor)?.FontColor;
                }
                bool containsCC = skuCode.Contains("CC");

                processedMessage.LogMessages.Add($"Quantity: {quantity}");
                processedMessage.LogMessages.Add(skuCode);
                processedMessage.LogMessages.Add(size);
                processedMessage.LogMessages.Add(color);
                var specialSku = _configManager.SpecialSkuCodes.Find(s => s.SkuCode == skuCode);

                IEnumerable<string> pngFiles;
                if (string.IsNullOrEmpty(fontColor))
                {
                    pngFiles = Directory.GetFiles(_configManager.SourceDirectoryPath, skuCode + "*.png");
                    processedMessage.WarningMessages.Add($"WARNING: Font color for Sku Code {skuCode} is missing. All colors will be considered. Please Delete the unwanted file one manualy on page {pageNumber}.");
                }
                else
                {
                    fontColor = fontColor.Trim();
                    var allFiles = Directory.GetFiles(_configManager.SourceDirectoryPath, skuCode + "*.png");

                    pngFiles = allFiles.Where(file =>
                        file.ToUpperInvariant().Contains(fontColor.ToUpperInvariant()) ||
                        file.ToUpperInvariant().Contains("-ALL.PNG"));

                    if (!pngFiles.Any())
                    {
                        processedMessage.WarningMessages.Add($"WARNING: Missing file with {fontColor} font color for Sku Code {skuCode} on page {pageNumber}. Considering '-ALL' files as well.");
                    }
                }

                foreach (var file in pngFiles)
                {
                    var matchDescriptor = Regex.Match(System.IO.Path.GetFileNameWithoutExtension(file), @"-?(FRONT|BACK|POCKET|SLEEVE|NECK)-?", RegexOptions.IgnoreCase);
                    var descriptor = matchDescriptor.Success ? matchDescriptor.Groups[1].Value : string.Empty;


                    // Check if color contains 'comfort' This is JUST FOR HULKI NOT RASIT
                    if (containsCC)
                    {
                        descriptor = "COMFORT_" + descriptor;
                    }


                    float sizeValue = 0;
                    SizeInfo matchedSizeInfo = CompareSizes(size, _configManager.AcceptableSizes);

                    if (descriptor.Contains("POCKET"))
                    {
                        if (matchedSizeInfo != null && matchedSizeInfo.PocketValue != null)
                        {
                           sizeValue = (float)matchedSizeInfo.PocketValue;
                        }

                        if (specialSku != null && specialSku.PocketValue != null)
                        {
                           sizeValue = (float)specialSku.PocketValue;
                        }
                    }
                    else if (descriptor.Contains("SLEEVE"))
                    {
                        if (matchedSizeInfo != null && matchedSizeInfo.SleeveValue != null)
                        {
                           sizeValue = (float)matchedSizeInfo.SleeveValue;
                        }

                        if (specialSku != null && specialSku.SleeveValue != null)
                        {
                           sizeValue = (float)specialSku.SleeveValue;
                        }
                    }
                    else if (descriptor.Contains("NECK"))
                    {
                        if (matchedSizeInfo != null && matchedSizeInfo.NeckValue != null)
                        {
                           sizeValue = (float)matchedSizeInfo.NeckValue;
                        }

                        if (specialSku != null && specialSku.NeckValue != null)
                        {
                           sizeValue = (float)specialSku.NeckValue;
                        }
                    }
                    else if (descriptor.Contains("FRONT"))
                    {
                        if (matchedSizeInfo != null && matchedSizeInfo.Value != null)
                        {
                           sizeValue = (float)matchedSizeInfo.Value;
                        }

                        if (specialSku != null && specialSku.SizeValue != null)
                        {
                           sizeValue = (float)specialSku.SizeValue;
                        }
                    }
                    else if (descriptor.Contains("BACK"))
                    {
                        if (matchedSizeInfo != null && matchedSizeInfo.Value != null)
                        {
                                sizeValue = (float)matchedSizeInfo.Value;
                        }

                        if (specialSku != null && specialSku.BackValue != null)
                        {
                           sizeValue = (float)specialSku.BackValue;
                        }
                    }
                    else
                    {
                        processedMessage.WarningMessages.Add($"WARNING: No descriptor (FRONT|BACK|POCKET|SLEEVE|NECK) found SKU Code {skuCode} on page {pageNumber}. Edit manually!");
                    }

                    if (size == null && color != null)
                    {
                        string warningMessage = $"WARNING: Size missing for SA Code {skuCode} on page {pageNumber}.";
                        if (!processedMessage.WarningMessages.Contains(warningMessage))
                        {
                            processedMessage.WarningMessages.Add(warningMessage);
                        }
                    }
                    else if (size != null && color == null)
                    {
                        string warningMessage = $"WARNING: Color missing for SA Code {skuCode} on page {pageNumber}.";
                        if (!processedMessage.WarningMessages.Contains(warningMessage))
                        {
                            processedMessage.WarningMessages.Add(warningMessage);
                        }
                    }
                    else if(size == null && color == null)
                    {
                        string warningMessage = $"WARNING: Both size and color missing for SA Code {skuCode} on page {pageNumber}.";
                        if (!processedMessage.WarningMessages.Contains(warningMessage))
                        {
                            processedMessage.WarningMessages.Add(warningMessage);
                        }
                    }

                    if (quantity == null)
                    {
                        string warningMessage = $"WARNING: Quantity is null for the {skuCode} there will be 0 as quantity on this item on page {pageNumber}.";
                        if (!processedMessage.WarningMessages.Contains(warningMessage))
                        {
                            processedMessage.WarningMessages.Add(warningMessage);
                        }
                    }
                    if (quantity > 20)
                    {
                        processedMessage.WarningMessages.Add($"WARNING: Quantity is {quantity}. Seems bigger than usual, check and edit on page {pageNumber}.");

                    }

                    ProcessedItem temp = new ProcessedItem
                    {
                        Descriptor = descriptor,
                        FontColor = fontColor,
                        SizeValue = sizeValue
                    };
                    item.ProcessedItems.Add(temp);
                }
                processedItems.Add(item);
            }
            return processedItems;
        }

        private static SizeInfo CompareSizes(string content, List<SizeInfo> sizes)
        {
            string normalizedContent = NormalizeSize(content);

            foreach (var sizeInfo in sizes)
            {
                string normalizedSize = NormalizeSize(sizeInfo.Size);

                if (string.Equals(normalizedContent, normalizedSize, StringComparison.OrdinalIgnoreCase))
                {
                    return sizeInfo;
                }

                int distance = LevenshteinDistance(normalizedContent, normalizedSize);
                if (distance <= 2)
                {
                    string[] contentParts = normalizedContent.Split(new[] { '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] sizeParts = normalizedSize.Split(new[] { '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (contentParts.Length > 1 && sizeParts.Length > 1 && 
                        contentParts[0].Equals(sizeParts[0], StringComparison.OrdinalIgnoreCase) &&
                        contentParts[1].Equals(sizeParts[1], StringComparison.OrdinalIgnoreCase))
                    {
                       return sizeInfo;
                    }
                }
            }

            return null; 
        }

        private static string NormalizeSize(string size)
        {
            if (string.IsNullOrEmpty(size))
            {
                Console.WriteLine($"WARNING: Check the size manually, there is something wrong."); //TODO: add log

                return string.Empty;
            }
            return Regex.Replace(size, "[­\\-\\s/]", "").ToLowerInvariant();
        }
        public static int LevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
                return string.IsNullOrEmpty(t) ? 0 : t.Length;

            if (string.IsNullOrEmpty(t))
                return s.Length;

            int[] costs = new int[t.Length + 1];
            for (int i = 0; i <= s.Length; i++)
            {
                int lastValue = i;
                for (int j = 0; j <= t.Length; j++)
                {
                    if (i == 0)
                        costs[j] = j;
                    else
                    {
                        if (j > 0)
                        {
                            int newValue = costs[j - 1];
                            if (s[i - 1] != t[j - 1])
                                newValue = Math.Min(Math.Min(newValue, lastValue), costs[j]) + 1;
                            costs[j - 1] = lastValue;
                            lastValue = newValue;
                        }
                    }
                }
                if (i > 0)
                    costs[t.Length] = lastValue;
            }
            return costs[t.Length];
        }

    }
}
