using System.Globalization;
using System.Windows.Data;

namespace Common.Lib.UI.Converters
{
	public class ParenthesizingConverter : IValueConverter
	{
		public char Opener { get; set; } = '(';
		public char Closer { get; set; } = ')';

		public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) return null;
			return $"{Opener}{value}{Closer}";
		}

		public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string s && s.StartsWith(Opener) && s.EndsWith(Closer)) return s.Substring(1, s.Length - 2);
			return value;
		}
	}
}
