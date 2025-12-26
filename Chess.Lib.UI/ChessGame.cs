using Chess.Lib.Games;
using Common.Lib.UI;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Chess.Lib.UI
{
	public class ChessGame : GameViewBase
	{
		static ChessGame()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ChessGame), new FrameworkPropertyMetadata(typeof(ChessGame)));
		}

		public static readonly DependencyProperty BoardDimensionProperty = DependencyProperty.Register("BoardDimensions", typeof(double),
			typeof(ChessGame), new PropertyMetadata(800.0));

		public double BoardDimension
		{
			get => (double)GetValue(BoardDimensionProperty);
			set => SetValue(BoardDimensionProperty, value);
		}

		protected override void UseTemplate() { }
	}

	internal class BoardDimensionConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is double d) return 0.2 * d;
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
