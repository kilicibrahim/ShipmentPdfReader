using CommunityToolkit.Mvvm.Messaging;
using ShipmentPdfReader.Services.Pdf;
using ShipmentPdfReader.ViewModels;

namespace ShipmentPdfReader
{
    public partial class HomePage : ContentPage
    {
        //private string _selectedFilePath;

        [Obsolete]
        public HomePage()
        {
            InitializeComponent();
            WeakReferenceMessenger.Default.Register<Messages>(this, OnMessageReceived);

            BindingContext = new HomeViewModel();
        }
        private void OnMessageReceived(object sender, Messages message)
        {
            DisplayAlert("Warning", message.Value, "OK");
        }
        private async void OnItemTapped(object sender, EventArgs e)
        {
            var item = (ExtractedData)((StackLayout)sender).BindingContext;
            await Navigation.PushAsync(new DetailPage(item));
        }

    }
}