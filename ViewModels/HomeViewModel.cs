using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.Messaging;
using ShipmentPdfReader.Helpers;
using ShipmentPdfReader.Services.Pdf;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ShipmentPdfReader.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        public IDispatcher Dispatcher
        {
            get; set;
        }
        private int currentPage = 1;
        private readonly int itemsPerPage = 10;
        private List<string> allWarnings;
        private ObservableCollection<string> paginatedWarnings;
        public ObservableCollection<string> PaginatedWarnings
        {
            get => paginatedWarnings;
            set
            {
                paginatedWarnings = value;
                OnPropertyChanged(nameof(PaginatedWarnings));
            }
        }
        public ICommand NextPageCommand
        {
            get;
        }
        public ICommand PreviousPageCommand
        {
            get;
        }
        public Command StartProcessingCommand {  get; }
        public Command CreatePngsCommand
        {
            get;
        }

        public HomeViewModel()
        {
            StartProcessingCommand = new Command(async () => await StartProcessing());
            CreatePngsCommand = new Command(async () => await CreatePngs());
            NextPageCommand = new Command(NextPage);
            PreviousPageCommand = new Command(PreviousPage);
            allWarnings = new List<string>();
            UpdatePaginatedWarnings();
        }
        private int TotalPages;

        public string TotalPagesInfo
        {
            get => $"Page: {CurrentPage} / {TotalPages}";
        }
        public int CurrentPage
        {
            get => currentPage;
            set
            {
                if (currentPage != value)
                {
                    currentPage = value;
                    OnPropertyChanged(nameof(CurrentPage));
                    UpdatePaginatedWarnings();
                }
            }
        }
        public void NextPage()
        {
            if (CurrentPage * itemsPerPage < allWarnings.Count)
            {
                CurrentPage++;
            }
        }

        public void PreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
            }
        }

        private void UpdatePaginatedWarnings()
        {
            var itemsToShow = allWarnings
                .Skip((CurrentPage - 1) * itemsPerPage)
                .Take(itemsPerPage);
            PaginatedWarnings = new ObservableCollection<string>(itemsToShow);
            OnPropertyChanged(nameof(TotalPages));
            OnPropertyChanged(nameof(TotalPagesInfo));
        }
        private Task CreatePngs()
        {
            var extractedPagesData = ExtractedPagesData;
            if(extractedPagesData == null)
            {
                WeakReferenceMessenger.Default.Send(new Messages("You need to Process the Pdf first."));
                return Task.CompletedTask;
            }
            PngExtractor pngExtractor = new PngExtractor();
            pngExtractor.CreatePngs(extractedPagesData);
            WeakReferenceMessenger.Default.Send(new Messages("PNGs created successfully!"));

            return Task.CompletedTask;
        }

        public ICommand ToggleExpandCommand => new Command<Item>(item =>
        {
            item.IsExpanded = !item.IsExpanded;
        });
        public ObservableCollection<string> LogMessages { get; } = new ObservableCollection<string>();
        public ObservableCollection<ExtractedData> ExtractedPagesData { get; private set; } = new ObservableCollection<ExtractedData>();
        public ObservableCollection<string> WarningMessages { get; private set; } = new ObservableCollection<string>();
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
                    { DevicePlatform.MacCatalyst, new[] { "pdf" } }
                });

                var options = new PickOptions
                {
                    PickerTitle = "Please select a PDF file",
                    FileTypes = fileType
                };

                var result = await FilePicker.PickAsync(options);
                if (result != null && !string.IsNullOrWhiteSpace(result.FullPath))
                {
                    UpdatePath(ref _selectedFilePath, result.FullPath, key);
                }
                else
                {
                    WeakReferenceMessenger.Default.Send(new Messages("PDF file selection was cancelled."));
                }
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new Messages($"PDF selection failed: {ex.Message}"));
                Console.WriteLine($"PDF selection failed: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }

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
                var processedPageData = await pdfProcessor.ProcessPdfAsync();
                var newExtractedData = processedPageData.Select(p => p.Extracted).ToList();
                var newWarningMessages = processedPageData.SelectMany(p => p.Processed.WarningMessages).ToList();

                await Dispatcher.DispatchAsync(() =>
                {
                    DisposeAndClearCollection(ExtractedPagesData);
                    DisposeAndClearCollection(PaginatedWarnings);
                    ExtractedPagesData = new ObservableCollection<ExtractedData>(newExtractedData);
                    allWarnings = new List<string>(newWarningMessages);
                    CurrentPage = 1;
                    TotalPages = (int)Math.Ceiling(allWarnings.Count / (double)itemsPerPage);
                    UpdatePaginatedWarnings();

                    OnPropertyChanged(nameof(ExtractedPagesData));
                    OnPropertyChanged(nameof(PaginatedWarnings));
                });
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new Messages($"Failed to process the PDF. Please try again or contact support, with the following message: {ex}"));
            }
        }
        private void DisposeAndClearCollection<T>(ObservableCollection<T> collection)
        {
            if (collection != null)
            {
                foreach (var item in collection)
                {
                    if (item is IDisposable disposableItem)
                    {
                        disposableItem.Dispose();
                    }
                }
                collection.Clear();
            }
        }
    }
}
