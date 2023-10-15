using CommunityToolkit.Mvvm.Messaging;
using ShipmentPdfReader.ViewModels;

namespace ShipmentPdfReader
{
    public partial class SettingsPage : ContentPage
    {

        public SettingsPage()
        {
            InitializeComponent();
            WeakReferenceMessenger.Default.Register<Messages>(this, OnMessageReceived);

            BindingContext = new SettingsViewModel();
        }
        private void OnMessageReceived(object sender, Messages message)
        {
            DisplayAlert("Warning", message.Value, "OK");
        }
    }
}