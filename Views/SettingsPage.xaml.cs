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
          private void OnSidebarSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = e.CurrentSelection.FirstOrDefault() as string;

            // Clear the content area
            ContentArea.Children.Clear();

            // Add relevant content based on selected item
            switch (selectedItem)
            {
                case "General":
                    ContentArea.Children.Add(new Label { Text = "This is the beta.", FontSize = 24 });
                    break;

                case "Select Directory":
                    ContentArea.Children.Add(new Label { Text = "Output Directory Path:", FontAttributes = FontAttributes.Bold });
                    ContentArea.Children.Add(new Label { Text = ((SettingsViewModel)BindingContext).OutputDirectoryPath, FontAttributes = FontAttributes.Italic });
                    ContentArea.Children.Add(new Button { BackgroundColor = Color.FromArgb("#6200ea"), TextColor = Color.FromArgb("#FFFFFF"), HorizontalOptions = LayoutOptions.Start, MaximumWidthRequest = 250, Text = "Select Output Directory", Command = ((SettingsViewModel)BindingContext).SelectOutputDirectoryCommand });

                    ContentArea.Children.Add(new Label { Text = "Source Directory Path:", FontAttributes = FontAttributes.Bold });
                    ContentArea.Children.Add(new Label { Text = ((SettingsViewModel)BindingContext).SourceDirectoryPath, FontAttributes = FontAttributes.Italic });
                    ContentArea.Children.Add(new Button { BackgroundColor = Color.FromArgb("#6200ea"), TextColor = Color.FromArgb("#FFFFFF") , HorizontalOptions = LayoutOptions.Start, MaximumWidthRequest = 250, Text = "Select Source Directory", Command = ((SettingsViewModel)BindingContext).SelectSourceDirectoryCommand });
                    break;

                case "Import Configurations":
                    ContentArea.Children.Add(new Button { HorizontalOptions = LayoutOptions.Start, MaximumWidthRequest = 250, Text = "Import Size Info", Command = ((SettingsViewModel)BindingContext).ImportSizeConfigurationCommand });
                    ContentArea.Children.Add(new Button { HorizontalOptions = LayoutOptions.Start, MaximumWidthRequest = 250, Text = "Import Color Info", Command = ((SettingsViewModel)BindingContext).ImportColorConfigurationCommand });
                    ContentArea.Children.Add(new Button { HorizontalOptions = LayoutOptions.Start, MaximumWidthRequest = 250, Text = "Import Special Sku Info", Command = ((SettingsViewModel)BindingContext).ImportSkuConfigurationCommand });
                    break;

                case "Export Configurations":
                    ContentArea.Children.Add(new Button { HorizontalOptions = LayoutOptions.Start, MaximumWidthRequest = 250, Text = "Export Configuration", Command = ((SettingsViewModel)BindingContext).ExportConfigurationCommand });
                    break;
            }
        }
    }
}