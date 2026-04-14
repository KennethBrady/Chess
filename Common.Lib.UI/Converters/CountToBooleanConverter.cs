using Common.Lib.Extensions;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace Common.Lib.UI.Converters
{
	public class CountToBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is ICollection c) return c.Count > 0;
			if (value is IEnumerable e) return e.Count > 0;
			return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
