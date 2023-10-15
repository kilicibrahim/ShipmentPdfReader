using ShipmentPdfReader.ViewModels;

namespace ShipmentPdfReader
{
    public partial class SizeInfoPage : BaseInfoPage
    {
        public SizeInfoPage()
        {
            InitializeComponent();

            BindingContext = new SizeInfoViewModel(ConfigurationManager.Instance.AcceptableSizes, ConfigurationManager.Instance);
        }

    }
}