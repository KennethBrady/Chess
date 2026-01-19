using System.Globalization;
using System.Windows.Data;

namespace Common.Lib.UI.Converters
{
	public class DoubleToStringConverter : IValueConverter
	{
		public DoubleToStringConverter()
		{
			MinValue = double.MinValue;
			MaxValue = double.MaxValue;
			NDec = -1;
		}

		public double MinValue { get; set; }
		public double MaxValue { get; set; }
		public int NDec { get; set; }
		public bool ValueIsPercentage { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is double d)) return string.Empty;
			d = Qualify(d);
			int nDec = (NDec < 0) ? 2 : NDec;
			if (parameter is string sdec && int.TryParse(sdec, out int nd)) nDec = nd;
			d *= PercentageFactor;
			System.Diagnostics.Debug.WriteLine(d);
			return d.ToString($"F{nDec}");
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string sval)
			{
				if (sval == ".") return 0;
				if (double.TryParse(sval, out double r)) return Qualify(r * RevPercentageFactor);
				if (string.IsNullOrEmpty(sval)) return 0;
			}
			return double.NaN;
		}

		private double PercentageFactor => ValueIsPercentage ? 100.0 : 1.0;
		private double RevPercentageFactor => ValueIsPercentage ? 0.01 : 1.0;

		private double Qualify(double v)
		{
			if (v < MinValue) return MinValue;
			if (v > MaxValue) return MaxValue;
			return v;
		}
	}
}
