using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.Messaging;

namespace ShipmentPdfReader.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public Command SelectOutputDirectoryCommand { get; }
        public Command SelectSourceDirectoryCommand { get; }
        public SettingsViewModel()
        {
            SelectOutputDirectoryCommand = new Command(async () => await SelectPath("OutputDirectoryPath", isFile: false));
            SelectSourceDirectoryCommand = new Command(async () => await SelectPath("SourceDirectoryPath", isFile: false));
            _sourceDirectoryPath = Preferences.Get("SourceDirectoryPath", string.Empty);
            _outputDirectoryPath = Preferences.Get("OutputDirectoryPath", string.Empty);
        }

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
