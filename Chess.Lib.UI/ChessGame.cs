using Chess.Lib.Games;
using Chess.Lib.Hardware.Timing;
using Chess.Lib.Moves;
using Chess.Lib.UI.Dialogs;
using Common.Lib.UI;
using Common.Lib.UI.Dialogs;
using Common.Lib.UI.Windows;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Chess.Lib.UI
{
	public class ChessGame : GameViewBase
	{
		static ChessGame()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ChessGame), new FrameworkPropertyMetadata(typeof(ChessGame)));
			DefaultPauseGameStyle = new Style { TargetType = typeof(Border) };
			DefaultPauseGameStyle.Setters.Add(new Setter(Border.BackgroundProperty, Brushes.Black));
		}

		private static readonly Style DefaultPauseGameStyle;

		public static readonly DependencyProperty BoardDimensionProperty = DependencyProperty.Register("BoardDimensions", typeof(double),
			typeof(ChessGame), new PropertyMetadata(800.0));

		public static readonly DependencyProperty PauseGameBorderStyleProperty = DependencyProperty.Register("PauseGameBorderStyle",
			typeof(Style), typeof(ChessGame), new PropertyMetadata(null, null, CoercePauseGameBorderStyle));

		private static object? CoercePauseGameBorderStyle(DependencyObject d, object value)
		{
			return value is Style s && s.TargetType == typeof(Border) ? s : null;
		}

		public double BoardDimension
		{
			get => (double)GetValue(BoardDimensionProperty);
			set => SetValue(BoardDimensionProperty, value);
		}

		public Style PauseGameBorderStyle
		{
			get => (Style)GetValue(PauseGameBorderStyleProperty);
			set => SetValue(PauseGameBorderStyleProperty, value);
		}


		private Border PauseBorder { get; set; } = DefaultControls.Border;
		protected override void UseTemplate() 
		{
			PauseBorder = (Border)GetTemplateChild("pauseBorder");
			if (PauseBorder.Style == null) PauseBorder.Style = (PauseGameBorderStyle == null) ? DefaultPauseGameStyle : PauseGameBorderStyle;
			Button resume = (Button)GetTemplateChild("resumeBtn");
			resume.Click += (s, e) =>
			{
				if (Game is IInteractiveChessGame ig) ig.Clock.Resume();
			};
		}

		protected override void ApplyGame(Games.IChessGame oldGame, IChessGame newGame)
		{
			base.ApplyGame(oldGame, newGame);
			if (newGame is IInteractiveChessGame ig)
			{
				ig.PromotionRequest += Ig_PromotionRequest;
				if (ig.Clock is not INoClock) ig.Clock.StateChanged += Clock_StateChanged;
			}
		}

		private async Task<Promotion> Ig_PromotionRequest(Promotion value)
		{
			PromotionDialogModel pdm = new PromotionDialogModel(value, Mouse.GetPosition(this));
			IAppWindow window = (IAppWindow)Window.GetWindow(this);
			IDialogResultAccepted<Promotion> acc = (IDialogResultAccepted<Promotion>)(await window.ShowDialog(pdm));
			return acc.Value;
		}

		private void Clock_StateChanged(ClockStateChange value)
		{
			if (value.IsPauseOrUnpause)
			{
				if (value.Current.HasFlag(ClockState.Paused))
				{
					PauseBorder.Visibility = Visibility.Visible;
				}
				else
				{
					PauseBorder.Visibility= Visibility.Collapsed;
				}
			}
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
