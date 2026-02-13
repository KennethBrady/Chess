using Chess.Lib.Games;
using Chess.Lib.Moves;
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
			newGame.GameStateApplied += HandleGameStateApplied;
			if (newGame is IInteractiveChessGame ig) ig.MoveUndone += HandleMoveUndone;
		}

		protected virtual void HandleGameStateApplied(IChessgameState value) { }

		protected virtual void HandleMoveCompleted(CompletedMove move) { }

		protected virtual void HandleMoveUndone(IChessMove undoneMove) { }
	}
}
