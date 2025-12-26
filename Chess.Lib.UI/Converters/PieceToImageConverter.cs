using Chess.Lib.Hardware.Pieces;
using Chess.Lib.UI.Images;
using System.Globalization;
using System.Windows.Data;

namespace Chess.Lib.UI.Converters
{
	public class PieceToImageConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is IChessPiece piece) return ImageLoader.LoadImage(piece)!;
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
