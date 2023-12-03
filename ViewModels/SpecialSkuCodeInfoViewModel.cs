using ShipmentPdfReader.Models;
using CommunityToolkit.Mvvm.Messaging;

namespace ShipmentPdfReader.ViewModels
{
    public class SpecialSkuCodeInfoViewModel : BaseInfoViewModel<SpecialSkuCodeInfo>
    {
        private readonly ConfigurationManager _configManager;

        public SpecialSkuCodeInfoViewModel(List<SpecialSkuCodeInfo> specialSKUCodes, ConfigurationManager configManager)
            : base(specialSKUCodes, () => new SpecialSkuCodeInfo())
        {
            _configManager = configManager;
        }

        protected override void SaveConfigurationsToFile()
        {
            try
            {
                string filename = "specialSKUCodes.json";
                _configManager.SaveJsonToFile(_configurations.ToList(), filename);
                WeakReferenceMessenger.Default.Send(new Messages("Data saved successfully!"));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                WeakReferenceMessenger.Default.Send(new Messages("Failed to save data. Please try again!"));
            }
        }
        protected override bool IsValidEntry(SpecialSkuCodeInfo entry)
        {
            bool isSkuCodeValid = !string.IsNullOrWhiteSpace(entry.SkuCode);

            bool isAtLeastOneOtherPropertySet =
                entry.SizeValue.HasValue ||
                entry.BackValue.HasValue ||
                entry.PocketValue.HasValue ||
                entry.SleeveValue.HasValue ||
                entry.NeckValue.HasValue;

            return isSkuCodeValid && isAtLeastOneOtherPropertySet;
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
