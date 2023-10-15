using ShipmentPdfReader.ViewModels;

namespace ShipmentPdfReader
{
    public partial class SpecialSkuCodeInfoPage : BaseInfoPage
    {
        public SpecialSkuCodeInfoPage()
        {
            InitializeComponent();

            BindingContext = new SpecialSkuCodeInfoViewModel(ConfigurationManager.Instance.SpecialSkuCodes, ConfigurationManager.Instance);
        }

    }
}