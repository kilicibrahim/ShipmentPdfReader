using ShipmentPdfReader.Models;
using CommunityToolkit.Mvvm.Messaging;

namespace ShipmentPdfReader.ViewModels
{
    public class SizeInfoViewModel : BaseInfoViewModel<SizeInfo>
    {
        private ConfigurationManager _configManager;

        public SizeInfoViewModel(List<SizeInfo> acceptableSizes, ConfigurationManager configManager)
            : base(acceptableSizes, () => new SizeInfo())
        {
            _configManager = configManager;
        }

        protected override void SaveConfigurationsToFile()
        {
            try
            {
                string filename = "acceptableSizes.json";
                _configManager.SaveJsonToFile(_configurations.ToList(), filename);
                WeakReferenceMessenger.Default.Send(new Messages("Data saved successfully!"));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                WeakReferenceMessenger.Default.Send(new Messages("Failed to save data. Please try again!"));
            }
        }
        protected override bool IsValidEntry(SizeInfo entry)
        {
            bool isSizeEntryValid = !string.IsNullOrWhiteSpace(entry.Size);

            bool isAtLeastOneOtherPropertySet =
                entry.Value.HasValue ||
                entry.PocketValue.HasValue ||
                entry.SleeveValue.HasValue;

            return isSizeEntryValid && isAtLeastOneOtherPropertySet;
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
