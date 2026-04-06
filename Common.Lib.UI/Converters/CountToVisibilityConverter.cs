using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Common.Lib.UI.Converters
{
	public class CountToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) return Visibility.Collapsed;
			if (value is ICollection coll) return coll.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
