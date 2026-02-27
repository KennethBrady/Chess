using Chess.Lib.Hardware;
using System.Globalization;
using System.Windows.Data;

namespace Chess.Lib.UI.Converters
{
	public class HueToBrushConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Hue h)
			{
				switch(h)
				{
					case Hue.White: return ChessBoardProperties.LightSquareBrush;
					case Hue.Black: return ChessBoardProperties.DarkSquareBrush;
				}
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
