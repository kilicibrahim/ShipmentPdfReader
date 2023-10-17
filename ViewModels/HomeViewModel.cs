using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.Messaging;
using ShipmentPdfReader.Services.Pdf;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ShipmentPdfReader.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        public Command StartProcessingCommand {  get; }

        [Obsolete]
        public HomeViewModel()
        {
            StartProcessingCommand = new Command(async () => await StartProcessing());
        }
        public ICommand ToggleExpandCommand => new Command<Item>(item =>
        {
            item.IsExpanded = !item.IsExpanded;
        });
        public ObservableCollection<string> LogMessages { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> WarningMessages { get; } = new ObservableCollection<string>();
        public ObservableCollection<ExtractedData> ExtractedPagesData { get; set; } = new ObservableCollection<ExtractedData>();
        public ICommand NavigateToDetailPageCommand => new Command<ExtractedData>(async (data) => await NavigateToDetailPage(data));

        private async Task NavigateToDetailPage(ExtractedData data)
        {
            await Application.Current.MainPage.Navigation.PushAsync(new DetailPage(data));
        }

        private string _selectedFilePath;
        public string SelectedFilePath
        {
            get => _selectedFilePath;
            set => UpdatePath(ref _selectedFilePath, value, "selectedFilePath");
        }
        private void UpdatePath(ref string field, string value, string key)
        {
            if (field != value)
            {
                field = value;
                SavePath(key, value);
                OnPropertyChanged(key);
            }
        }
        private void SavePath(string key, string path)
        {
            Preferences.Set(key, path);
        }
        private async Task SelectPath(string key)
        {
            try
            {
                var fileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".pdf" } },
                    { DevicePlatform.Android, new[] { "application/pdf" } },
                    { DevicePlatform.iOS, new[] { "com.adobe.pdf" } },
                    { DevicePlatform.MacCatalyst, new[] { "pdf" } }
                });

                var options = new PickOptions
                {
                    PickerTitle = "Please select a PDF file",
                    FileTypes = fileType
                };

                var result = await FilePicker.PickAsync(options);
                if (result != null)
                {
                    UpdatePath(ref _selectedFilePath, result.FullPath, key);
                }
            } 
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new Messages($"Pdf selection failed: {ex.Message}"));
                Console.WriteLine($"Pdf selection failed: {ex.Message}");
            }
        }

        [Obsolete]
        private async Task StartProcessing()
        {
            var sourceDirectoryPath = Preferences.Get("sourceDirectoryPath", string.Empty);
            var outputDirectoryPath = Preferences.Get("outputDirectoryPath", string.Empty);

            if (string.IsNullOrEmpty(sourceDirectoryPath) || string.IsNullOrEmpty(outputDirectoryPath))
            {
                WeakReferenceMessenger.Default.Send(new Messages("Source Directory or Output Directory is not selected. Please select in Setting tab."));
                return;
            }

            await SelectPath("selectedFilePath");

            if (string.IsNullOrWhiteSpace(_selectedFilePath))
            {
                WeakReferenceMessenger.Default.Send(new Messages("Select a Pdf to Start Processing"));
                return;
            }

            try
            {
                PdfProcessor pdfProcessor = new PdfProcessor(_selectedFilePath);
                var processedPageData = pdfProcessor.ProcessPdf(); 

                // Ensure UI updates are performed on the main thread
                await Device.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var processedPage in processedPageData)
                    {
                        ExtractedPagesData.Add(processedPage.Extracted);
                        foreach (var warningMessage in processedPage.Processed.WarningMessages)
                        {
                            WarningMessages.Add(warningMessage);
                        }

                    }


                });
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new Messages($"Failed to process the PDF. Please try again or contact support! With the following message: {ex}"));
            }

            return;
        }
    }
}
