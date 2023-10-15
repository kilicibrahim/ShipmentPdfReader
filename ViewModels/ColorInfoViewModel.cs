using ShipmentPdfReader.Models;
using CommunityToolkit.Mvvm.Messaging;

namespace ShipmentPdfReader.ViewModels
{
    public class ColorInfoViewModel : BaseInfoViewModel<ColorInfo>
    {
        private ConfigurationManager _configManager;

        public ColorInfoViewModel(List<ColorInfo> acceptableColors, ConfigurationManager configManager)
            : base(acceptableColors, () => new ColorInfo())
        {
            _configManager = configManager;
        }

        protected override void SaveConfigurationsToFile()
        {
            try
            {
                string filename = "acceptableColors.json";
                _configManager.SaveJsonToFile(_configurations.ToList(), filename);
                WeakReferenceMessenger.Default.Send(new Messages("Data saved successfully!"));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                WeakReferenceMessenger.Default.Send(new Messages("Failed to save data. Please try again!"));
            }
        }
        protected override bool IsValidEntry(ColorInfo entry)
        {
            return !string.IsNullOrWhiteSpace(entry.FontColor) &&
                   !string.IsNullOrWhiteSpace(entry.BackgroundColor);
        }
        protected override void AddNewEntry()
        {
            try
            {
                Configurations.Add(NewEntry);
                NewEntry = _createModelInstance(); 
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
