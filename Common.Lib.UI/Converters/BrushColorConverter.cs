using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Common.Lib.UI.Converters
{
	public class BrushColorConverter : IValueConverter
	{
		public static Color DefaultColor { get; set; } = Colors.Black;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is SolidColorBrush b) return b.Color;
			if (value is Color c) return new SolidColorBrush(c);
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is SolidColorBrush b) return b.Color;
			if (value is Color c) return new SolidColorBrush(c);
			return value;
		}
	}
}
