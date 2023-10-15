namespace ShipmentPdfReader.Models
{
    public class SpecialSkuCodeInfo : ObservableObject
    {

        private string _SkuCode;
        private float? _sizeValue;
        private float? _backValue;
        private float? _pocketValue;
        private float? _sleeveValue;
        private bool? _isColorSelectionManual;
        public string SkuCode
        {
            get => _SkuCode;
            set => SetProperty(ref _SkuCode, value);
        }

        public float? SizeValue
        {
            get => _sizeValue;
            set => SetProperty(ref _sizeValue, value);
        }
        public float? BackValue
        {
            get => _backValue;
            set => SetProperty(ref _backValue, value);
        }
        public float? PocketValue
        {
            get => _pocketValue;
            set => SetProperty(ref _pocketValue, value);
        }
        public float? SleeveValue
        {
            get => _sleeveValue;
            set => SetProperty(ref _sleeveValue, value);
        }
        public bool IsColorSelectionManual
        {
            get => _isColorSelectionManual ?? false;
            set => SetProperty(ref _isColorSelectionManual, value);
        }
    }
}
