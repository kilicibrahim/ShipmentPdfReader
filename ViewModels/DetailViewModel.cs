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
        public ICommand NavigateToPngDetailPageCommand => new Command<Item>(async (data) => await NavigateToPngDetailPage(data));
        private async Task NavigateToPngDetailPage(Item data)
        {
            await Application.Current.MainPage.Navigation.PushAsync(new PngDetailPage(data));
        }
        private void ApplyChanges(object obj)
        {
            throw new NotImplementedException();
        }

        public DetailViewModel(ExtractedData data)
        {
            Data = data;
            WeakReferenceMessenger.Default.Send(new Messages("Data saved successfully!"));
        }
    }
}
