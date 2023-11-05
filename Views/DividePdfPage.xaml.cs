using CommunityToolkit.Mvvm.Messaging;
using ShipmentPdfReader.Services.Pdf;
using ShipmentPdfReader.ViewModels;

namespace ShipmentPdfReader
{
    public partial class DividePdfPage : ContentPage
    {

        public DividePdfPage()
        {
            InitializeComponent();
            WeakReferenceMessenger.Default.Register<Messages>(this, OnMessageReceived);
            BindingContext = new DividePdfViewModel();
        
        }
        private void OnMessageReceived(object sender, Messages message)
        {
            DisplayAlert("Warning", message.Value, "OK");
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            WeakReferenceMessenger.Default.Unregister<Messages>(this);
        }
    }
}