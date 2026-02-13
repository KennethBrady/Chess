using Chess.Lib.Games;
using Chess.Lib.Moves;
using Chess.Lib.UI.Dialogs;
using Common.Lib.UI.Dialogs;
using Common.Lib.UI.Windows;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

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

		protected override void ApplyGame(Games.IChessGame oldGame, Games.IChessGame newGame)
		{
			base.ApplyGame(oldGame, newGame);
			if (newGame is IInteractiveChessGame ig) ig.PromotionRequest += Ig_PromotionRequest;
		}

		private async Task<Promotion> Ig_PromotionRequest(Promotion value)
		{
			PromotionDialogModel pdm = new PromotionDialogModel(value, Mouse.GetPosition(this));
			IAppWindow window = (IAppWindow)Window.GetWindow(this);
			IDialogResultAccepted<Promotion> acc = (IDialogResultAccepted<Promotion>)(await window.ShowDialog(pdm));
			return acc.Value;
		}
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
