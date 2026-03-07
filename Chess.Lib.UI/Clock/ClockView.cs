using Chess.Lib.Games;
using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Timing;
using Common.Lib.UI;
using Common.Lib.UI.MVVM;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Chess.Lib.UI.Clock
{
	public class ClockView : Control
	{
		static ClockView()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ClockView), new FrameworkPropertyMetadata(typeof(ClockView)));
		}

		public static readonly DependencyProperty PlayerProperty = DependencyProperty.Register("Player", typeof(IChessPlayer),
			 typeof(ClockView), new PropertyMetadata(GameFactory.NoPlayer));

		public static readonly DependencyProperty WarningThresholdProperty = DependencyProperty.Register("WarningThreshold", typeof(double),
			typeof(ClockView), new PropertyMetadata(10.0));

		public static readonly DependencyProperty WarningBackgroundProperty = DependencyProperty.Register("WarningBackground", typeof(Brush),
			typeof(ClockView), new PropertyMetadata(Brushes.Red));

		public IChessPlayer Player
		{
			get => (IChessPlayer)GetValue(PlayerProperty);
			set => SetValue(PlayerProperty, value);
		}

		/// <summary>
		/// Get/Set the time below which the clock will show the warning background.
		/// </summary>
		public double WarningThreshold
		{
			get => (double)GetValue(WarningThresholdProperty);
			set => SetValue(WarningThresholdProperty, value);
		}

		public Brush WarningBackground
		{
			get => (Brush)GetValue(WarningBackgroundProperty);
			set => SetValue(WarningBackgroundProperty, value);
		}

		private Border Border { get; set; } = DefaultControls.Border;

		bool IsTemplateApplied { get; set; }
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			Border = (Border)GetTemplateChild("border");
			IsTemplateApplied = true;
			Apply();
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			if (IsTemplateApplied && e.Property == PlayerProperty) Apply();
		}

		private void Apply()
		{
			Border.DataContext = null;
			if (!IsTemplateApplied) return;
			if (Player.Game is not IInteractiveChessGame ig) return;
			if (Player is INoPlayer || ig.Clock is INoClock) return;
			Border.DataContext = new ClockModel(this, ig.Clock);
		}

		public class ClockModel : ViewModel
		{
			internal ClockModel(ClockView owner, IChessClock clock)
			{
				Owner = owner;
				Clock = clock;
				Clock.Tick += Clock_Tick;
				Clock.StateChanged += Clock_StateChanged;
				Foreground = Owner.Foreground;
				Background = Owner.Background;
				RemainingTime = ClockPlayer.Remaining.RemainingOnClock;
			}

			public bool IsActive => Player.HasNextMove;
			public string RemainingTime { get; private set; } = string.Empty;

			public IChessClock Clock { get; private init; }
			public IChessPlayer Player => Owner.Player;

			public Brush Foreground { get; private set; }
			public Brush Background { get; private set; }

			private IClockPlayer ClockPlayer => Player.Side == Hue.White ? Clock.White : Clock.Black;
			private ClockView Owner { get; init; }

			private void Clock_Tick(TimerTick value)
			{
				if (value.CurrentPlayer == Player.Side)
				{
					RemainingTime = value.Remaining.RemainingOnClock;
					if (value.Remaining.TotalSeconds <= Owner.WarningThreshold)
					{
						Foreground = Owner.WarningBackground;
						Notify(nameof(Foreground));
					}
					Notify(nameof(RemainingTime));					
				}
			}

			private void Clock_StateChanged(ClockStateChange value)
			{
				if (value.IsMoveMade) Notify(nameof(IsActive)); else
				if (value.IsFlagged && value.PlayerHue == Player.Side)
					{
						Background = Brushes.Red;
						Foreground = Owner.Foreground;
						Notify(nameof(Foreground), nameof(Background));
					}
			}
		}
	}

	internal class FontWeightConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool b) return b ? FontWeights.Bold : FontWeights.Normal;
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
