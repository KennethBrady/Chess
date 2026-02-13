using System.Globalization;
using System.Windows.Data;

namespace Common.Lib.UI.Converters
{
	public class IntToStringConverter : IValueConverter
	{
		public IntToStringConverter()
		{
			MinValue = int.MinValue;
			MaxValue = int.MaxValue;
		}

		public int MinValue { get; set; }
		public int MaxValue { get; set; }
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is int i)
			{
				return Qualify(i).ToString();
			}
			if (value is long l) return Qualify(l).ToString();
			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string s && int.TryParse(s, out int i))
			{
				return Qualify(i);
			}
			return 0;
		}

		private int Qualify(int i)
		{
			if (i > MaxValue) i = MaxValue;
			if (i < MinValue) i = MinValue;
			return i;
		}

		private long Qualify(long i)
		{
			if (i > MaxValue) i = MaxValue;
			if (i < MinValue) i = MinValue;
			return i;
		}
	}
}
