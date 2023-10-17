using Newtonsoft.Json;
using ShipmentPdfReader.Services.Pdf;
using ShipmentPdfReader.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics;

namespace ShipmentPdfReader
{
    public partial class DetailPage : ContentPage
    {
        public DetailPage(ExtractedData item)
        {
            InitializeComponent();
            BindingContext = item;
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            this.Navigation.PopAsync();
        }
    }
}
