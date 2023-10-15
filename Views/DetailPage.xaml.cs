using Newtonsoft.Json;
using ShipmentPdfReader.Services.Pdf;
using ShipmentPdfReader.ViewModels;

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
            this.Navigation.PopAsync(); // You might adjust navigation based on your app structure
        }
    }
}
