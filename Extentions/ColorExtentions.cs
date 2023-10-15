namespace ShipmentPdfReader.Extentions
{
    public static class ColorExtensions
    {
        public static Color Lighten(this Color color, double factor)
        {
            return Color.FromRgba(
                color.Alpha,
                (byte)Math.Min(255, color.Red + (255 - color.Red) * factor),
                (byte)Math.Min(255, color.Green + (255 - color.Green) * factor),
                (byte)Math.Min(255, color.Blue + (255 - color.Blue) * factor)
            );
        }

        public static Color Darken(this Color color, double factor)
        {
            return Color.FromRgba(
                color.Alpha,
                (byte)(color.Red * factor),
                (byte)(color.Green * factor),
                (byte)(color.Blue * factor)
            );
        }
    }
}
