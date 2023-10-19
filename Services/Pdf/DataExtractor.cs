using ShipmentPdfReader.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace ShipmentPdfReader.Services.Pdf
{
    internal class DataExtractor
    {
        private readonly ConfigurationManager _configManager;

        public DataExtractor()
        {
            _configManager = ConfigurationManager.Instance;
        }

        public List<PageData> ExtractData(Dictionary<int, string> pdfContentPages)
        {
            List<PageData> pagesData = new List<PageData>();

            foreach (var page in pdfContentPages)
            {
                ExtractedData extractedData = ExtractedPage(page);
                pagesData.Add(new PageData
                {
                    PageNumber = page.Key,
                    Extracted = extractedData
                });
            }

            return pagesData;
        }

        private ExtractedData ExtractedPage(KeyValuePair<int, string> page)
        {
            var extractedData = new ExtractedData();
            var text = page.Value;
            var orderCountPattern = @"(\d+)\n";
            var orderCountMatch = Regex.Match(text, orderCountPattern);
            var lines = text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (orderCountMatch.Success)
            {
                int orderCount = int.Parse(orderCountMatch.Value.Trim('-').Trim());
                extractedData.TotalItems = orderCount;
                Console.WriteLine($"Number of orders on the page: {orderCount}");
            }

            var pageNumberMatch = Regex.Match(text, @"Page (\d+) of");
            var pageNumber = pageNumberMatch.Groups[1].Value.Length > 0 ? Int32.Parse(pageNumberMatch.Groups[1].Value) : page.Key;
            extractedData.PageNumber = pageNumber;

            var itemData = new ItemData();

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var normalizedLine = line.ToLower().Replace("­", "").Replace("\u00A0", " ");
                if (normalizedLine.Contains("personalization:"))
                {
                    extractedData.IsPersonalized = true;
                }
                var quantityRegex = new Regex(@"^\d{1,2}( \d{1,2})?$", RegexOptions.Multiline);
                var quantitymatches = quantityRegex.Matches(line);

                var saMatch = Regex.Matches(line, @"(SA|CC|KT|SK|F)\d{4}|(AL|T|X|SM)\d{3}|(kuz)\d{2}|(kuz)\d{3}");
                var orderMatch = Regex.Match(line, @"­\s*(\d+)$");
                if (orderMatch.Success)
                {
                    int ordersInPage;
                    if (int.TryParse(orderMatch.Groups[1].Value, out ordersInPage))
                    {
                        // Now you have the number of orders in 'ordersInPage' variable.
                        Console.WriteLine($"Number of orders on the page: {ordersInPage}");
                    }
                }
                // Updated size and color match patterns
                // Update the regex patterns to include the colon ':'
                var sizePattern = @"(?:SIZE|S\u00A0I\u00A0Z\u00A0E|S\s*I\s*Z\s*E|S\s*I\s*Z\s*E\s*S|style|Style|s\s*t\s*y\s*l\s*e|S\u00A0T\u00A0Y\u00A0L\u00A0E):";
                var colorPattern = @"(?:COLORS|C\s*O\s*L\s*O\s*R|C\s*O\s*L\s*O\s*R\s*S):";

                var allPatterns = sizePattern + "|" + colorPattern;
                var matches = Regex.Matches(line, allPatterns, RegexOptions.IgnoreCase);

                StringBuilder partialSizeBuilder = new StringBuilder();

                for (int j = 0; j < matches.Count; j++)
                {
                    var startIdx = matches[j].Index + matches[j].Length;
                    var endIdx = j < matches.Count - 1 ? matches[j + 1].Index : line.Length;

                    // Sanity check for Substring
                    if (startIdx < 0 || endIdx <= startIdx || endIdx > line.Length)
                    {
                        Console.WriteLine($"WARNING: startIdx or endIdx out of range for line {line}. Skipping this match.");
                        continue;  // Skip to the next iteration
                    }

                    var splitContent = line.Substring(startIdx, endIdx - startIdx).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var content = (splitContent.Length > 0) ? splitContent[0].Trim().ToLower() : string.Empty;

                    if (Regex.IsMatch(matches[j].Value, sizePattern, RegexOptions.IgnoreCase))
                    {
                        if (partialSizeBuilder.Length > 0)
                        {
                            // If there's a partial size, prepend it
                            content = partialSizeBuilder.ToString() + " " + content;
                        }

                        // Check if the content is an acceptable size
                        if (IsAcceptableString(content, _configManager.AcceptableSizes))
                        {
                            itemData.Sizes.Add(content);
                            partialSizeBuilder.Clear(); // Clear the builder since we've found a complete size
                        }
                        else // The size might be partial, try it
                        {
                            partialSizeBuilder.Append(content);
                            if (partialSizeBuilder.Length > 0 && i < lines.Length - 1)
                            {
                                var nextLine = lines[i + 1];
                                var nextLineContent = nextLine.Trim().ToLower();
                                var seperator = ' ';
                                var listOfNextLineContent = nextLineContent.Split(seperator);
                                foreach (var s in listOfNextLineContent)
                                {
                                    var completeSize = partialSizeBuilder.ToString() + " " + s;

                                    if (IsAcceptableString(completeSize, _configManager.AcceptableSizes))
                                    {
                                        itemData.Sizes.Add(completeSize);
                                        partialSizeBuilder.Clear();
                                    }
                                    completeSize = null;
                                }
                            }
                        }
                    }
                    else if (Regex.IsMatch(matches[j].Value, colorPattern, RegexOptions.IgnoreCase))
                    {
                        var normalizedContent = content.ToLower().Trim().Replace("\u00A0", " ");
                        var normalizedBackgroundColorList = _configManager.AcceptableColors.Select(c => c.BackgroundColor.ToLower().Trim());
                        // Check if the content is an acceptable color
                        if (normalizedBackgroundColorList.Any(c => c == normalizedContent))
                        {
                            itemData.Colors.Add(content);
                        }
                    }
                }

                foreach (Match match in saMatch.Cast<Match>())
                {
                    itemData.SkuCodes.Add(match.Value);
                }
                foreach (Match match in quantitymatches.Cast<Match>())
                {
                    var quantities = match.Value.Split(' ').Select(int.Parse).ToList();
                    foreach (var quantity in quantities)
                    {
                        Console.WriteLine($"Quantity: {quantity}");
                        itemData.Quantities.Add(Convert.ToInt32(quantity));
                    }
                }
            }
            extractedData.ConvertToItems(itemData);
            return extractedData;
        }

        private bool IsAcceptableString(string content, List<SizeInfo> sizes)
        {
            // Normalize the content
            string normalizedContent = content
                .Replace("­", "")  // Remove soft hyphen
                .Replace("\u00A0", " ")  // Replace non-breaking spaces with regular spaces
                .ToLower();  // Convert to lower case

            return sizes.Any(sizeInfo =>
            {
                // Normalize the size
                string normalizedSize = sizeInfo.Size
                    .Replace("­", "")  // Remove soft hyphen
                    .Replace("\u00A0", " ")  // Replace non-breaking spaces with regular spaces
                    .ToLower();  // Convert to lower case

                return normalizedContent == normalizedSize ||
                       LevenshteinDistance(normalizedContent, normalizedSize) <= 1;
            });
        }
        private int LevenshteinDistance(string s, string t)
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
