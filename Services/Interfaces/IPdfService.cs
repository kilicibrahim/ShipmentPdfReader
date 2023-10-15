using ShipmentPdfReader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipmentPdfReader.Services.Interfaces
{
    internal interface IPdfService
    {
        Task<ExtractedDataModel> ExtractDataFromPdf(string filePath);
    }
}
