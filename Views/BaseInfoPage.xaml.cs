using CommunityToolkit.Mvvm.Messaging;
using ShipmentPdfReader.ViewModels;

namespace ShipmentPdfReader
{
    public class BaseInfoPage : ContentPage
    //<TViewModel, TModel> : ContentPage // TODO: lastly, if you can find a workaround solution on xaml page taking generics
    //where TViewModel : BaseInfoViewModel<TModel>
    //where TModel : class, new()
    {
        //protected readonly TViewModel ViewModel;

        //public BaseInfoPage(TViewModel viewModel)
        //{
        //    ViewModel = viewModel;
        //    BindingContext = ViewModel;

        public BaseInfoPage()
        {
            WeakReferenceMessenger.Default.Register<Messages>(this, OnMessageReceived);
        }

        private void OnMessageReceived(object sender, Messages message)
        {
            DisplayAlert("Message", message.Value, "OK");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            WeakReferenceMessenger.Default.Unregister<Messages>(this);
        }
    }

}
