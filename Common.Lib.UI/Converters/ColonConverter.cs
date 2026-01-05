using System.Globalization;
using System.Windows.Data;

namespace Common.Lib.UI.Converters
{
	public class ColonConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string s)
			{
				if (parameter is string p) return s + p;
				return s + ":";
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
