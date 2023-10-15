using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipmentPdfReader.Models
{
    public class ExtractedDataModel
    {
        public List<string> SkuCodes { get; set; }
        public List<string> Sizes { get; set; }
        public List<string> Colors { get; set; }
    }

}
