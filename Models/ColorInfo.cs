namespace ShipmentPdfReader.Models
{
    public class ColorInfo : ObservableObject
    {
        private string _backgroundColor;
        private string _fontColor;

        public string BackgroundColor
        {
            get => _backgroundColor;
            set => SetProperty(ref _backgroundColor, value);
        }        
        public string FontColor
        {
            get => _fontColor;
            set => SetProperty(ref _fontColor, value);
        }
    }
}
