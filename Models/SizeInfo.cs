namespace ShipmentPdfReader.Models
{
    public class SizeInfo : ObservableObject
    {
        private string _size;
        private float? _value;
        private float? _pocketValue;
        private float? _sleeveValue;
        private float? _neckValue;

        public string Size
        {
            get => _size;
            set => SetProperty(ref _size, value);
        }

        public float? Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
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
        public float? NeckValue
        {
            get => _neckValue;
            set => SetProperty(ref _neckValue, value);
        }
    }

}
