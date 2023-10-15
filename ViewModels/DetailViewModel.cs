using CommunityToolkit.Mvvm.Messaging;
using ShipmentPdfReader.Services.Pdf;
using System.Windows.Input;

namespace ShipmentPdfReader.ViewModels
{
    public class DetailViewModel : BaseViewModel
    {
        private ExtractedData _data;

        public ExtractedData Data
        {
            get => _data;
            set => SetProperty(ref _data, value);
        }
        public ICommand ApplyChangesCommand => new Command(ApplyChanges);

        private void ApplyChanges(object obj)
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        public DetailViewModel(ExtractedData data)
        {
            Data = data;
            MessagingCenter.Send(this, "DataUpdated", Data);
            WeakReferenceMessenger.Default.Send(new Messages("Data saved successfully!"));
        }
    }
}
