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

            ContentArea.Children.Clear();

            switch (selectedItem)
            {
                case "General":
                    ContentArea.Children.Add(new Label { Text = "This is the beta. Please give us feedback!", FontSize = 24 });
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

                case "Sorting":
                    CreateSortingSettingsUI();
                    break;
            }
        }
        private void CreateSortingSettingsUI()
        {
            var vm = (SettingsViewModel)BindingContext;
            var sortingCheckBox = new CheckBox { IsChecked = vm.IsSortingEnabled };
            sortingCheckBox.SetBinding(CheckBox.IsCheckedProperty, nameof(vm.IsSortingEnabled));
            var checkBoxLabel = new Label { Text = "Enable Sorting" };
            var checkBoxLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = { sortingCheckBox, checkBoxLabel }
            };
            ContentArea.Children.Add(checkBoxLayout);
            var firstPicker = new Picker { Title = "First Sorting Criterion", ItemsSource = vm.SortOptions };
            firstPicker.SetBinding(Picker.SelectedItemProperty, nameof(vm.FirstSortCriterion));

            var secondPicker = new Picker { Title = "Second Sorting Criterion", ItemsSource = vm.SortOptions };
            secondPicker.SetBinding(Picker.SelectedItemProperty, nameof(vm.SecondSortCriterion));

            var thirdPicker = new Picker { Title = "Third Sorting Criterion", ItemsSource = vm.SortOptions };
            thirdPicker.SetBinding(Picker.SelectedItemProperty, nameof(vm.ThirdSortCriterion));

            ContentArea.Children.Add(firstPicker);
            ContentArea.Children.Add(secondPicker);
            ContentArea.Children.Add(thirdPicker);

            var saveButton = new Button { Text = "Save Sorting Settings" };
            saveButton.Clicked += (s, e) => vm.SaveSortSettings();
            ContentArea.Children.Add(saveButton);
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            WeakReferenceMessenger.Default.Unregister<Messages>(this);
        }
    }
}