using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Common.Lib.UI.Converters
{
	public class BooleanToBoldConverter : IValueConverter
	{
		public FontWeight Normal { get; set; } = FontWeights.Normal;
		public FontWeight Bold { get; set; } = FontWeights.Bold;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool b)
			{
				return b ? Bold : Normal;
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
