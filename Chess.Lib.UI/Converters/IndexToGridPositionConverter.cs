using System.Globalization;
using System.Windows.Data;

namespace Chess.Lib.UI.Converters
{
	/// <summary>
	/// Convert IChessSquare.Index to Grid.Row / Grid.Column
	/// </summary>
	public class IndexToGridPositionConverter : IValueConverter
	{
		public static int RowFor(int squareIndex)
		{
			if (squareIndex < 0 || squareIndex > 63) return -1;
			return (63 - squareIndex) / 8;
		}
		public static int ColumnFor(int squareIndex) => squareIndex % 8;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is int ndx && parameter is string cr)
			{
				switch(cr)
				{
					case "r":	return RowFor(ndx); // row in grid
					case "c": return ColumnFor(ndx);	// column in grid
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
