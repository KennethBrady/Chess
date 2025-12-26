using Chess.Lib.Games;
using Common.Lib.UI;
using System.Windows;

namespace Chess.Lib.UI
{
	public abstract class GameViewBase : ControlBase
	{
		protected const string GamePropertyName = "Game";
		private static readonly IChessGame DefaultGame = GameFactory.NoGame;

		public static readonly DependencyProperty GameProperty = DependencyProperty.Register(GamePropertyName, typeof(IChessGame),
			typeof(GameViewBase), new PropertyMetadata(DefaultGame, HandleDependencyPropertyChanged, CoerceGame));

		private static void HandleDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is GameViewBase v && e.Property.Name == GamePropertyName) v.ApplyGame((IChessGame)e.OldValue, (IChessGame)e.NewValue);
		}

		private static object CoerceGame(DependencyObject o, object baseValue) => baseValue == null ? DefaultGame : baseValue;

		public IChessGame Game
		{
			get => (IChessGame)GetValue(GameProperty);
			set => SetValue(GameProperty, value);
		}

		protected virtual void ApplyGame(IChessGame oldGame, IChessGame newGame)
		{
			newGame.MoveCompleted += HandleMoveCompleted;
		}

		protected virtual void HandleMoveCompleted(CompletedMove move) { }
	}
}
