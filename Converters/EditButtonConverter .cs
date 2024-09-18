using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ShipmentPdfReader.Converters
{
    public class EditButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "Save" : "Edit";
        }

        [Obsolete]
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Not needed here
            throw new NotImplementedException();
        }
    }
}
