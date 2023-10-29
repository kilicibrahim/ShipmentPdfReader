using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using ShipmentPdfReader.Models;
namespace ShipmentPdfReader.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public Command SelectOutputDirectoryCommand { get; }
        public Command SelectSourceDirectoryCommand { get; }
        public Command ExportConfigurationCommand
        {
            get;
        }
        public Command ImportSizeConfigurationCommand
        {
            get;
        }
        public Command ImportColorConfigurationCommand
        {
            get;
        }
        public Command ImportSkuConfigurationCommand
        {
            get;
        }

        private bool _isSortingEnabled;
        public bool IsSortingEnabled
        {
            get => _isSortingEnabled;
            set
            {
                SetProperty(ref _isSortingEnabled, value);
                SaveSortSettings();
            }
        }

        private string _firstSortCriterion;
        private string _secondSortCriterion;
        private string _thirdSortCriterion;

        public string FirstSortCriterion
        {
            get => _firstSortCriterion;
            set
            {
                if (string.IsNullOrEmpty(value) || value == SecondSortCriterion || value == ThirdSortCriterion)
                {
                    return;
                }
                SetProperty(ref _firstSortCriterion, value);
                SaveSortSettings();
            }
        }

        public string SecondSortCriterion
        {
            get => _secondSortCriterion;
            set
            {
                if (string.IsNullOrEmpty(value) || value == FirstSortCriterion || value == ThirdSortCriterion)
                {
                    return;
                }
                SetProperty(ref _secondSortCriterion, value);
                SaveSortSettings();
            }
        }

        public string ThirdSortCriterion
        {
            get => _thirdSortCriterion;
            set
            {
                if (string.IsNullOrEmpty(value) || value == FirstSortCriterion || value == SecondSortCriterion)
                {
                    return;
                }
                SetProperty(ref _thirdSortCriterion, value);
                SaveSortSettings();
            }
        }
        public List<string> SortOptions { get; } = new List<string> { "", "Sku", "Size", "Color" };

        private readonly ConfigurationManager _configurationManager;

        public SettingsViewModel()
        {
            SelectOutputDirectoryCommand = new Command(async () => await SelectPath("OutputDirectoryPath", isFile: false));
            SelectSourceDirectoryCommand = new Command(async () => await SelectPath("SourceDirectoryPath", isFile: false));
            ExportConfigurationCommand = new Command(async () => await ExportConfiguration());
            ImportColorConfigurationCommand = new Command(async () => await ImportConfiguration(ConfigurationType.Color));
            ImportSizeConfigurationCommand = new Command(async () => await ImportConfiguration(ConfigurationType.Size));
            ImportSkuConfigurationCommand = new Command(async () => await ImportConfiguration(ConfigurationType.Sku));
            _sourceDirectoryPath = Preferences.Get("SourceDirectoryPath", string.Empty);
            _outputDirectoryPath = Preferences.Get("OutputDirectoryPath", string.Empty);
            _configurationManager = ConfigurationManager.Instance;
            LoadSortSettings();
        }
        private void LoadSortSettings()
        {
            IsSortingEnabled = Preferences.Get("IsSortingEnabled", false);
            FirstSortCriterion = Preferences.Get("FirstSortCriterion", "");
            SecondSortCriterion = Preferences.Get("SecondSortCriterion", "");
            ThirdSortCriterion = Preferences.Get("ThirdSortCriterion", "");
        }

        public void SaveSortSettings()
        {
            Preferences.Set("IsSortingEnabled", IsSortingEnabled);
            Preferences.Set("FirstSortCriterion", FirstSortCriterion ?? "");
            Preferences.Set("SecondSortCriterion", SecondSortCriterion ?? "");
            Preferences.Set("ThirdSortCriterion", ThirdSortCriterion ?? "");
        }
        public List<string> SidebarItems { get; } = new List<string>
        {
            "General",
            "Select Directory",
            "Import Configurations",
            "Export Configurations",
            "Sorting"
        };
        private string _outputDirectoryPath;

        public string OutputDirectoryPath
        {
            get => _outputDirectoryPath;
            set => UpdatePath(ref _outputDirectoryPath, value, "OutputDirectoryPath");
        }

        private string _sourceDirectoryPath;
        public string SourceDirectoryPath
        {
            get => _sourceDirectoryPath;
            set => UpdatePath(ref _sourceDirectoryPath, value, "SourceDirectoryPath");
        }
        private async Task ExportConfiguration()
        {
            try
            {
                var result = await FolderPicker.PickAsync(new CancellationToken());
                if (result.IsSuccessful)
                {
                    ConfigurationManager.Instance.BatchExportConfigurations(result.Folder.Path);
                }
            }
            catch (Exception ex)
            {
                // Handle the exception
                WeakReferenceMessenger.Default.Send(new Messages($"Exporting configurations failed: {ex.Message}"));
            }
        }
        private async Task ImportConfiguration(ConfigurationType configType)
        {
            try
            {
                var fileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".json" } },
                    { DevicePlatform.Android, new[] { "application/json" } },
                    { DevicePlatform.iOS, new[] { ".json" } },
                    { DevicePlatform.MacCatalyst, new[] { "json" } }
                });

                var options = new PickOptions
                {
                    PickerTitle = "Please select a Json file",
                    FileTypes = fileType
                };
                var result = await FilePicker.PickAsync(options);
                if (result.ContentType == "application/json")
                {
                    ConfigurationManager.Instance.ImportConfiguration(result.FullPath, configType);
                    WeakReferenceMessenger.Default.Send(new Messages($"Configuration imported successfully!"));
                }
                else
                {
                    WeakReferenceMessenger.Default.Send(new Messages($"File selection failed. Please make sure you selected the right configuration file."));
                }
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new Messages($"Import failed: {ex.Message}"));
            }
        }

        private void UpdatePath(ref string field, string value, string propertyName)
        {
            if (field != value)
            {
                field = value;
                SavePath(propertyName, value);
                OnPropertyChanged(propertyName);
            }
        }
        private void SavePath(string key, string path)
        {
            Preferences.Set(key, path);
        }
        private async Task SelectPath(string key, bool isFile)
        {
            try
            {
                string initialPath = "";
                CancellationToken cancellationToken = new();

                var result = await FolderPicker.PickAsync(initialPath, cancellationToken);
                if (result.IsSuccessful)
                {
                    if (key == "outputDirectoryPath")
                        UpdatePath(ref _outputDirectoryPath, result.Folder.Path, key);
                    else
                        UpdatePath(ref _sourceDirectoryPath, result.Folder.Path, key);
                }
                else
                {
                    WeakReferenceMessenger.Default.Send(new Messages($"Directory selection failed"));
                }
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new Messages($"Directory selection failed: {ex.Message}"));
                Console.WriteLine($"{(isFile ? "File" : "Folder")} selection failed: {ex.Message}");
            }
        }
    }
}
