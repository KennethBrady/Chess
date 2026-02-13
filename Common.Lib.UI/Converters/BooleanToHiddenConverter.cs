using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Common.Lib.UI.Converters
{
	public class BooleanToHiddenConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is not bool b) return value;
			return b ? Visibility.Visible : Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is not Visibility v) return value;
			return v == Visibility.Visible;
		}
	}
}
