using Chess.Lib.Games;
using Chess.Lib.Moves;
using Common.Lib.MVVM;
using Common.Lib.UI;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Chess.Lib.UI
{
	public class MovesView : GameViewBase
	{
		static MovesView()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(MovesView), new FrameworkPropertyMetadata(typeof(MovesView)));
		}

		private DataGrid Moves { get; set; } = DefaultControls.DataGrid;
		private ObservableCollection<MovePair> _moves = new();
		protected override void UseTemplate()
		{
			Moves = (DataGrid)GetTemplateChild("moves");
			Moves.ItemsSource = _moves;
		}

		protected override void HandleMoveCompleted(CompletedMove move)
		{
			base.HandleMoveCompleted(move);
			if (move.Move.Side == Hardware.Hue.Light) _moves.Add(new MovePair(move.Move)); else _moves.Last().ApplyBlack(move.Move);
		}

		public class MovePair : ViewModel
		{
			private IChessMove _white = GameFactory.NoMove, _black = GameFactory.NoMove;

			internal MovePair(IChessMove whiteMove)
			{
				_white = whiteMove;
				MoveNumber = 1 + _white.SerialNumber / 2;
			}

			internal void ApplyBlack(IChessMove move)
			{
				_black = move;
				Notify(nameof(BlackMove));
			}

			public int MoveNumber { get; private init; }

			public IChessMove WhiteMove => _white;

			public IChessMove BlackMove => _black;
		}
	}
}
