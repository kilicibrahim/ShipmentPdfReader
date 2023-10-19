using Newtonsoft.Json;
using ShipmentPdfReader.Services.Pdf;
using ShipmentPdfReader.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics;

namespace ShipmentPdfReader
{
    public partial class PngDetailPage : ContentPage
    {
        public PngDetailPage(Item item)
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
