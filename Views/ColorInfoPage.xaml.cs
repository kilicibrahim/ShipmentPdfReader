using ShipmentPdfReader.ViewModels;

namespace ShipmentPdfReader
{
    public partial class ColorInfoPage : BaseInfoPage
    {
        public ColorInfoPage()
        {
            InitializeComponent();

            BindingContext = new ColorInfoViewModel(ConfigurationManager.Instance.AcceptableColors, ConfigurationManager.Instance);
        }

    }
}